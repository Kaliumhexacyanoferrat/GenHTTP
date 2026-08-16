using System.Buffers;
using System.IO.Pipelines;
using System.Net;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Engine.Shared.Types;

using Glyph11.Parser;
using Glyph11.Parser.UltraHardened;
using Glyph11.Pico;
using Glyph11.Protocol;

using Microsoft.Extensions.Logging;

using Connection = GenHTTP.Api.Protocol.Connection;
using IoConnection = ioxide.TcpConnection;

namespace GenHTTP.Engine.Ioxide.Protocol;

/// <summary>
/// Serves an HTTP/1.1 connection: parse, handle, respond, repeat until it closes. Reached from
/// <see cref="ConnectionDriver"/> once the transport is up and the protocol settled.
/// </summary>
/// <remarks>
/// One of these per connection, on the reactor thread, with awaited continuations resuming inline
/// on that same thread - which is what lets the request pool below go without locking. Unlike
/// HTTP/2 and HTTP/3 there is one request in flight at a time, so a connection owns a single
/// <see cref="Request"/> for its lifetime.
/// </remarks>
internal static class Http1Driver
{
    private static readonly ParserLimits Limits = ParserLimits.Default;

    // Benchmark switch: GENHTTP_IOXIDE_PARSER=pico parses headers with Glyph11.Pico
    // (picohttpparser, native) instead of the hardened managed parser. Both fill the same
    // BinaryRequest. The Pico path does picohttpparser-level validation only (no path/token/
    // smuggling hardening), so it is for benchmarking, not for untrusted traffic.
    private static readonly bool UsePico =
        string.Equals(Environment.GetEnvironmentVariable("GENHTTP_IOXIDE_PARSER"), "pico", StringComparison.OrdinalIgnoreCase);

    private static readonly ByteString KeepAliveValue = new("Keep-Alive");

    // Per-reactor, so the stack needs no locking. Reuses the per-connection Request allocation,
    // which matters under connection churn.
    [ThreadStatic]
    private static Stack<Request>? _requestPool;

    private const int MaxPooledRequests = 1024;

    internal static async Task RunAsync(IServer server, IEndPoint endPoint, IDuplexPipe pipe, IoConnection conn, IPAddress? remoteAddress)
    {
        var reader = pipe.Input;
        var writer = pipe.Output;

        var request = RentRequest();
        var into = request.Source;

        // Captured before the first await, so the diagnostic below can tell whether continuations
        // really did resume on the reactor thread the pool assumes.
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

                // Client cert stays null until TLS termination exposes one.
                request.Apply(server, endPoint, reader, buffer.Start, remoteAddress, null);

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
            WarnIfThreadHopped(server, reactorThreadId, "before-return");

            await ConnectionDriver.CloseAsync(pipe, conn);

            ReturnRequest(request);
        }
    }

    private static bool TryParseRequest(ref ReadOnlySequence<byte> buffer, BinaryRequest into)
        => UsePico ? TryParseRequestPico(ref buffer, into) : TryParseRequestGlyph11(ref buffer, into);

    // Default: full RFC + smuggling validation.
    private static bool TryParseRequestGlyph11(ref ReadOnlySequence<byte> buffer, BinaryRequest into)
    {
        if (!UltraHardenedParser.TryExtractFullHeaderValidated(ref buffer, into, Limits, out var bytesRead))
        {
            return false;
        }

        buffer = buffer.Slice(bytesRead + 1);
        return true;
    }

    // picohttpparser: single-segment is parsed in place, multi-segment is linearized. `consumed`
    // follows the managed parser's -1 convention, so the slice is identical.
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

        var keepAliveRequested = true;

        if (connectionHeader is not null)
        {
            keepAliveRequested = connectionHeader == KeepAliveValue;
        }
        else if (header.Protocol == HttpProtocol.Http10)
        {
            keepAliveRequested = false;
        }

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

    private static int _hopWarned;

    // Warns once per process if a continuation resumed off the reactor thread. On the affine path
    // it is a single int compare, so it can stay enabled during benchmarks.
    private static void WarnIfThreadHopped(IServer server, int reactorThreadId, string phase)
    {
        var now = Environment.CurrentManagedThreadId;

        if (now == reactorThreadId || _hopWarned != 0)
        {
            return;
        }

        if (Interlocked.Exchange(ref _hopWarned, 1) == 0)
        {
            server.Logging.CreateLogger("GenHTTP.Engine.Ioxide.Protocol.Http1Driver")
                  .LogWarning("Thread hop detected: reactor={ReactorThreadId} now={CurrentThreadId} phase={Phase}. " +
                              "The [ThreadStatic] Request pool assumes reactor affinity; pooling degrades under work-stealing. (warns once)", reactorThreadId, now, phase);
        }
    }
}
