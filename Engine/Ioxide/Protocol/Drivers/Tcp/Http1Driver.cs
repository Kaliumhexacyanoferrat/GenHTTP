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
using GenHTTP.Engine.Ioxide.Protocol.Responses;

namespace GenHTTP.Engine.Ioxide.Protocol.Drivers.Tcp;

/// <summary>Serves HTTP/1.1 on one connection, request after request.</summary>
internal static class Http1Driver
{
    private static readonly ParserLimits Limits = ParserLimits.Default;

    // GENHTTP_IOXIDE_PARSER=pico swaps the hardened managed parser for picohttpparser, which
    // does no path/token/smuggling hardening. For benchmarking, not for untrusted traffic.
    private static readonly bool UsePico =
        string.Equals(Environment.GetEnvironmentVariable("GENHTTP_IOXIDE_PARSER"), "pico", StringComparison.OrdinalIgnoreCase);

    private static readonly ByteString KeepAliveValue = new("Keep-Alive");

    [ThreadStatic]
    private static Stack<Request>? _requestPool;

    private const int MaxPooledRequests = 1024;

    // Serves requests off one connection until the client closes it, keep-alive says no, or the server stops.
    internal static async Task RunAsync(IServer server, IEndPoint endPoint, IDuplexPipe pipe, IoConnection conn, IPAddress? remoteAddress)
    {
        var reader = pipe.Input;
        var writer = pipe.Output;

        var request = RentRequest();
        var into = request.Source;

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
        finally
        {
            WarnIfThreadHopped(server, reactorThreadId, "before-return");

            await TcpDriver.CloseAsync(pipe, conn);

            ReturnRequest(request);
        }
    }

    // Parses one request head, through whichever parser this process was started with.
    private static bool TryParseRequest(ref ReadOnlySequence<byte> buffer, BinaryRequest into)
        => UsePico ? TryParseRequestPico(ref buffer, into) : TryParseRequestGlyph11(ref buffer, into);

    // The default path: full RFC validation plus smuggling hardening.
    private static bool TryParseRequestGlyph11(ref ReadOnlySequence<byte> buffer, BinaryRequest into)
    {
        if (!UltraHardenedParser.TryExtractFullHeaderValidated(ref buffer, into, Limits, out var bytesRead))
        {
            return false;
        }

        buffer = buffer.Slice(bytesRead + 1);
        return true;
    }

    // The benchmark path: picohttpparser, which validates only as far as picohttpparser does.
    private static bool TryParseRequestPico(ref ReadOnlySequence<byte> buffer, BinaryRequest into)
    {
        if (!PicoParser.TryParse(buffer, into, out var consumed))
        {
            return false;
        }

        buffer = buffer.Slice(consumed + 1);
        return true;
    }

    // Runs one request through the handler chain and writes the response; returns whether the connection lives on.
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

        await Http1Responder.WriteAsync(writer, request, response, keepAliveRequested && !closeRequested, headRequest);

        return keepAliveRequested && !closeRequested;
    }

    // Takes a request off this reactor's pool, or makes one.
    private static Request RentRequest()
        => _requestPool is { } pool && pool.TryPop(out var request) ? request : new Request();

    // Puts a reset request back, up to the pool's ceiling.
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

    // Warns once per process if a continuation resumed off the reactor thread the request pool assumes.
    private static void WarnIfThreadHopped(IServer server, int reactorThreadId, string phase)
    {
        var now = Environment.CurrentManagedThreadId;

        if (now == reactorThreadId || _hopWarned != 0)
        {
            return;
        }

        if (Interlocked.Exchange(ref _hopWarned, 1) == 0)
        {
            server.Logging.CreateLogger("GenHTTP.Engine.Ioxide.Protocol.Drivers.Tcp.Http1Driver")
                  .LogWarning("Thread hop detected: reactor={ReactorThreadId} now={CurrentThreadId} phase={Phase}. " +
                              "The [ThreadStatic] Request pool assumes reactor affinity; pooling degrades under work-stealing. (warns once)", reactorThreadId, now, phase);
        }
    }
}
