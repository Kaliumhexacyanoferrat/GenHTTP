using System.Buffers;
using System.IO.Pipelines;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Engine.Shared.Types;

using Glyph11.Parser;
using Glyph11.Parser.UltraHardened;
using Glyph11.Pico;
using Glyph11.Protocol;

using Microsoft.Extensions.Logging;

using Connection = GenHTTP.Api.Protocol.Connection;
using IoConnection = ioxide.Connection;

namespace GenHTTP.Engine.Ioxide.Protocol;

/// <summary>
/// Drives GenHTTP's parse -> handle -> respond loop over an ioxide connection. One instance of this
/// runs per accepted connection on the reactor thread; awaited continuations resume inline on that
/// same thread.
/// </summary>
internal static class ConnectionDriver
{
    private static readonly ParserLimits Limits = ParserLimits.Default;

    // Benchmark switch: set GENHTTP_IOXIDE_PARSER=pico to parse request headers with
    // Glyph11.Pico (picohttpparser, native) instead of the hardened managed Glyph11 parser.
    // Both fill the same BinaryRequest, so the rest of the pipeline is identical — only the
    // header-parsing implementation differs. NOTE: the Pico path does picohttpparser-level
    // validation only (no path/token/smuggling hardening); it's for benchmarking, not for
    // hardening untrusted traffic.
    private static readonly bool UsePico =
        string.Equals(Environment.GetEnvironmentVariable("GENHTTP_IOXIDE_PARSER"), "pico", StringComparison.OrdinalIgnoreCase);

    private static readonly ReadOnlyMemory<byte> KeepAliveValue = "Keep-Alive"u8.ToArray();

    // Half-close (SHUT_WR = 1) the socket's write side to send FIN. ioxide's refcounted teardown does not
    // FIN a server-initiated close by itself (the reactor's active recv keeps a reference), so an
    // EOF-delimited response (connection-close / upgrade) would otherwise hang the client. The read side
    // stays open so the client's own close is still observed and the reactor reclaims the connection.
    private const int ShutWrite = 1;

    [System.Runtime.InteropServices.DllImport("libc", EntryPoint = "shutdown")]
    private static extern int Shutdown(int sockfd, int how);

    // Per-reactor pool of Request objects. Each reactor runs on its own thread and services its
    // connections cooperatively, so the stack needs no locking. Reuses the per-connection Request
    // allocation, which matters under connection churn (e.g. limited-conn). Mirrors the Internal
    // engine's ClientContext pool, adapted for thread-per-core.
    [ThreadStatic]
    private static Stack<Request>? _requestPool;

    private const int MaxPooledRequests = 1024;

    internal static async Task HandleAsync(IServer server, IEndPoint endPoint, IoConnection conn, Func<IoConnection, ValueTask<IDuplexPipe>>? connectionFactory)
    {
        // Default transport is a plain duplex pipe over the connection. A connectionFactory (e.g. the
        // TLS-terminating one supplied by the host for the :8081 listener) can swap in a transport that
        // decrypts inbound bytes and writes plaintext for kTLS TX.
        var pipe = connectionFactory is null ? new ioxide.ConnectionDualPipe(conn) : await connectionFactory(conn);

        var reader = pipe.Input;
        var writer = pipe.Output;

        var request = RentRequest();
        var into = request.Source;

        // Diagnostic: the [ThreadStatic] Request pool and lock-free pooling assume every continuation
        // in this method resumes on the reactor thread that entered it. Capture that thread now — before
        // the first await — so we can warn (once) if ioxide ever resumes us on a different thread
        // (e.g. under a work-stealing scheduler), which silently degrades the pool.
        var reactorThreadId = Environment.CurrentManagedThreadId;

        try
        {
            var dataRemaining = false;
            ReadResult readResult = default;

            while (server.Running)
            {
                if (!dataRemaining)
                {
                    readResult = await reader.ReadAsync();
                    WarnIfThreadHopped(server, reactorThreadId, "after-read");
                }

                dataRemaining = false;

                var buffer = readResult.Buffer;

                if (!TryParseRequest(ref buffer, into))
                {
                    reader.AdvanceTo(readResult.Buffer.Start, readResult.Buffer.End);
                    if (readResult.IsCompleted)
                    {
                        break;
                    }
                    continue;
                }

                // todo: connection does not seem to expose IP address and client cert currently
                request.Apply(server, endPoint, reader, buffer.Start, null, null);

                var keepAlive = await HandleRequestAsync(server, writer, request);
                WarnIfThreadHopped(server, reactorThreadId, "after-handle");

                if (!keepAlive)
                {
                    await writer.FlushAsync();
                    break;
                }

                await request.DrainAsync();
                request.Reset();

                if (readResult.IsCompleted)
                {
                    break;
                }

                if (reader.TryRead(out var next)) // pipeline mode (more data available)
                {
                    readResult = next;
                    dataRemaining = true;
                }
                else
                {
                    await writer.FlushAsync();
                }
            }
        }
        catch
        {
            // spike: swallow client/protocol faults; teardown happens in finally
        }
        finally
        {
            await reader.CompleteAsync();

            // Signal end-of-writes so the client sees EOF. Length-delimited responses don't strictly
            // need this, but connection-close and upgrade (101) responses are delimited by FIN — without
            // completing the writer the client blocks waiting for bytes that never come. Mirrors the
            // reader completion above.
            await writer.CompleteAsync();

            WarnIfThreadHopped(server, reactorThreadId, "before-return");
            if (pipe is IAsyncDisposable disposable)
            {
                await disposable.DisposeAsync(); // tears down a TLS transport (stops the decrypt pump, close_notify)
            }
            // Send FIN for server-initiated closes so EOF-delimited responses terminate for the client
            // (see ShutWrite above). Harmless for client-initiated closes — the fd is already closing.
            Shutdown(conn.ClientFd, ShutWrite);
            conn.DecRef();
            ReturnRequest(request);
        }
    }

    private static bool TryParseRequest(ref ReadOnlySequence<byte> buffer, BinaryRequest into)
        => UsePico ? TryParseRequestPico(ref buffer, into) : TryParseRequestGlyph11(ref buffer, into);

    // Hardened managed parser (default): full RFC + smuggling validation.
    private static bool TryParseRequestGlyph11(ref ReadOnlySequence<byte> buffer, BinaryRequest into)
    {
        if (!UltraHardenedParser.TryExtractFullHeaderValidated(ref buffer, into, Limits, out var bytesRead))
        {
            return false;
        }

        buffer = buffer.Slice(bytesRead + 1);
        return true;
    }

    // picohttpparser (native) — single-segment is parsed in place, multi-segment is linearized.
    // `consumed` follows the same -1 convention as the managed parser, so the slice is identical.
    private static bool TryParseRequestPico(ref ReadOnlySequence<byte> buffer, BinaryRequest into)
    {
        if (!PicoParser.TryParse(buffer, into, out var consumed))
        {
            return false;
        }

        buffer = buffer.Slice(consumed + 1);
        return true;
    }

    private static async ValueTask<bool> HandleRequestAsync(IServer server, PipeWriter writer, Request request)
    {
        var header = request.Header;

        var headRequest = header.Method == RequestMethod.Head;

        var connectionHeader = header.Headers.GetEntry(KnownHeaders.Connection);

        var keepAliveRequested = connectionHeader?.Bytes.Span.SequenceEqual(KeepAliveValue.Span) ?? (header.Protocol == HttpProtocol.Http11);

        var response = await server.Handler.HandleAsync(request) ?? throw new InvalidOperationException("The root request handler did not return a response");

        var closeRequested = response.Mode is Connection.Close or Connection.Upgrade;

        await ResponseWriter.WriteAsync(writer, request, response, keepAliveRequested && !closeRequested, headRequest);

        return keepAliveRequested && !closeRequested;
    }

    private static Request RentRequest()
        => _requestPool is { } pool && pool.TryPop(out var request) ? request : new Request();

    private static void ReturnRequest(Request request)
    {
        request.Reset();

        var pool = _requestPool ??= new Stack<Request>();

        if (pool.Count < MaxPooledRequests)
        {
            pool.Push(request);
        }
    }

    // Set to 1 the first time a continuation is seen resuming off the reactor thread.
    private static int _hopWarned;

    // Warns at most once per process if this phase's continuation resumed on a different thread than the
    // reactor thread that entered HandleAsync. On the fast (affine) path it's a single int compare, so it's
    // safe to leave enabled during benchmarks — the one-shot guard keeps it from perturbing throughput.
    private static void WarnIfThreadHopped(IServer server, int reactorThreadId, string phase)
    {
        var now = Environment.CurrentManagedThreadId;

        if (now == reactorThreadId || _hopWarned != 0)
        {
            return;
        }

        if (Interlocked.Exchange(ref _hopWarned, 1) == 0)
        {
            server.Logging.CreateLogger("GenHTTP.Engine.Ioxide.Protocol.ConnectionDriver")
                  .LogWarning("Thread hop detected: reactor={ReactorThreadId} now={CurrentThreadId} phase={Phase}. " +
                              "The [ThreadStatic] Request pool assumes reactor affinity; pooling degrades under work-stealing. (warns once)", reactorThreadId, now, phase);
        }
    }
    
}
