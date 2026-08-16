using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Runtime.InteropServices;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Engine.Ioxide;
using GenHTTP.Engine.Ioxide.Protocol.Multiplexed;
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
/// Drives GenHTTP's parse -> handle -> respond loop over an ioxide connection. One instance of this
/// runs per accepted connection on the reactor thread; awaited continuations resume inline on that
/// same thread.
/// </summary>
internal static partial class ConnectionDriver
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


    /// <summary>
    /// Half-close (SHUT_WR = 1) the socket's write side to send FIN. ioxide's refcounted teardown does not
    /// FIN a server-initiated close by itself (the reactor's active recv keeps a reference), so an
    /// EOF-delimited response (connection-close / upgrade) would otherwise hang the client. The read side
    /// stays open so the client's own close is still observed and the reactor reclaims the connection.
    /// </summary>
    private const int ShutWrite = 1;

    [LibraryImport("libc", EntryPoint = "shutdown")]
    private static partial int Shutdown(int sockfd, int how);

    [LibraryImport("libc", EntryPoint = "getpeername")]
    private static partial int GetPeerName(int sockfd, [Out] byte[] addr, ref int addrlen);

    private static readonly ByteString KeepAliveValue = new("Keep-Alive");

    // The connected client's remote address, read once per connection straight from the socket fd
    // (ioxide exposes the fd but not the peer address). Mirrors the Internal engine's
    // Socket.RemoteEndPoint.Address - returned as-is (IPv4-mapped IPv6 on a dual-stack listener), which
    // IPAddress.IsLoopback and the rest of the pipeline already handle.
    private static IPAddress? GetPeerAddress(int fd)
    {
        var addr = new byte[128]; // sockaddr_storage
        var len = addr.Length;

        if (GetPeerName(fd, addr, ref len) != 0)
        {
            return null;
        }

        var family = addr[0] | (addr[1] << 8);

        return family switch
        {
            2 => new IPAddress(addr[4..8]), // AF_INET  -> sin_addr
            10 => new IPAddress(addr[8..24]), // AF_INET6 -> sin6_addr
            _ => null,
        };
    }

    // Per-reactor pool of Request objects. Each reactor runs on its own thread and services its
    // connections cooperatively, so the stack needs no locking. Reuses the per-connection Request
    // allocation, which matters under connection churn (e.g. limited-conn). Mirrors the Internal
    // engine's ClientContext pool, adapted for thread-per-core.
    [ThreadStatic] private static Stack<Request>? _requestPool;

    private const int MaxPooledRequests = 1024;

    internal static async Task HandleAsync(IServer server, IEndPoint endPoint, IoConnection conn, Func<IoConnection, ValueTask<IDuplexPipe>>? connectionFactory, IoxideProtocols protocols = IoxideProtocols.Http1)
    {
        IDuplexPipe pipe;

        // What ALPN settled on, when the transport negotiated anything. Null on a plaintext port,
        // and on a TLS port whose client offered nothing we serve.
        string? negotiated = null;

        try
        {
            if (connectionFactory is not null)
            {
                pipe = await connectionFactory(conn);
            }
            else if (endPoint.Secure)
            {
                // A secure port with no certificate (an SNI-only provider yielded none) is advertised
                // for redirects but cannot handshake - FIN the connection so the client's handshake
                // fails fast rather than a plaintext response landing on an https port.
                if (!IoxideReactor.Current.GetService<TlsRegistry>().TryFor(conn.ListenerPort, out var service))
                {
                    _ = Shutdown(conn.ClientFd, ShutWrite);
                    conn.DecRef();
                    return;
                }

                (pipe, negotiated) = await IoxideTls.AcceptWithAlpnAsync(conn, service);
            }
            else
            {
                pipe = new ioxide.TcpConnectionDualPipe(conn);
            }
        }
        catch
        {
            // failed handshake (or factory fault) - release the connection instead of leaking it
            conn.DecRef();
            return;
        }

        var reader = pipe.Input;
        var writer = pipe.Output;

        // The peer address is constant for the connection; resolve it once from the socket fd.
        var remoteAddress = GetPeerAddress(conn.ClientFd);

        var http2 = protocols.HasFlag(IoxideProtocols.Http2);
        var http1 = protocols.HasFlag(IoxideProtocols.Http1);

        // Only worth peeking when both share the port. On an HTTP/2-only port every connection is
        // HTTP/2 by definition, and on an HTTP/1.1-only port the preface would be a malformed
        // request line either way.
        var isHttp2 = http2 && (negotiated == "h2" || (negotiated is null && http1 && await StartsWithPrefaceAsync(reader)));

        if (http2 && !http1)
        {
            isHttp2 = true;
        }

        if (isHttp2)
        {
            // HTTP/2 owns the connection from here: it multiplexes, so there is no request loop to
            // run above it and nothing of the HTTP/1.1 path applies.
            try
            {
                await Http2Driver.RunAsync(server, endPoint, pipe, remoteAddress, endPoint.Secure);
            }
            catch
            {
                // client or protocol fault - teardown happens below
            }
            finally
            {
                await CloseAsync(pipe, conn);
            }

            return;
        }

        // The port does not serve HTTP/1.1, and this connection is not HTTP/2 - which on a secure
        // port means the client offered no ALPN this endpoint accepts. Close rather than answer it
        // with a protocol the endpoint was configured not to speak.
        if (!http1)
        {
            await CloseAsync(pipe, conn);
            return;
        }

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

                // Client cert stays null until TLS termination lands; the remote address is read from the socket.
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

    /// <summary>
    /// Peeks for the HTTP/2 connection preface without consuming it, so a plaintext client using
    /// prior knowledge (h2c) is recognised and the same bytes are handed to the HTTP/2 layer.
    /// </summary>
    private static async ValueTask<bool> StartsWithPrefaceAsync(PipeReader reader)
    {
        while (true)
        {
            var result = await reader.ReadAsync();
            var buffer = result.Buffer;

            if (buffer.Length >= Preface.Length)
            {
                Span<byte> head = stackalloc byte[Preface.Length];
                buffer.Slice(0, Preface.Length).CopyTo(head);

                // Nothing consumed AND nothing examined: marking these bytes examined would tell the
                // pipe we are waiting for more, and whichever protocol reads next would block on data
                // that has already arrived.
                reader.AdvanceTo(buffer.Start, buffer.Start);

                return head.SequenceEqual(Preface.Span);
            }

            // Too short to decide yet - examined to the end, so the next read waits for more.
            reader.AdvanceTo(buffer.Start, buffer.End);

            if (result.IsCompleted)
            {
                return false;
            }
        }
    }

    private static readonly ReadOnlyMemory<byte> Preface = "PRI * HTTP/2.0\r\n\r\nSM\r\n\r\n"u8.ToArray();

    private static async ValueTask CloseAsync(IDuplexPipe pipe, IoConnection conn)
    {
        await pipe.Input.CompleteAsync();
        await pipe.Output.CompleteAsync();

        if (pipe is IAsyncDisposable disposable)
        {
            await disposable.DisposeAsync();
        }

        Shutdown(conn.ClientFd, ShutWrite);
        conn.DecRef();
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
