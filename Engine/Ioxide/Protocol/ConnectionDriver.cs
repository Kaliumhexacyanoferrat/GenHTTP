using System.Buffers;
using System.IO.Pipelines;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Engine.Shared.Types;

using Glyph11.Parser;
using Glyph11.Parser.UltraHardened;
using Glyph11.Protocol;

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

    private static readonly ReadOnlyMemory<byte> KeepAliveValue = "Keep-Alive"u8.ToArray();

    // Per-reactor pool of Request objects. Each reactor runs on its own thread and services its
    // connections cooperatively, so the stack needs no locking. Reuses the per-connection Request
    // allocation, which matters under connection churn (e.g. limited-conn). Mirrors the Internal
    // engine's ClientContext pool, adapted for thread-per-core.
    [ThreadStatic]
    private static Stack<Request>? _requestPool;

    private const int MaxPooledRequests = 1024;

    internal static async Task HandleAsync(IServer server, IEndPoint endPoint, IoConnection conn)
    {
        var pipe = new ioxide.ConnectionDualPipe(conn);

        var reader = pipe.Input;
        var writer = pipe.Output;

        var request = RentRequest();
        var into = request.Source;

        try
        {
            var dataRemaining = false;
            ReadResult readResult = default;

            while (server.Running)
            {
                if (!dataRemaining)
                {
                    readResult = await reader.ReadAsync();
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

                request.Apply(server, endPoint, reader, buffer.Start);

                var keepAlive = await HandleRequestAsync(server, writer, request);

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
            conn.DecRef();
            ReturnRequest(request);
        }
    }

    private static bool TryParseRequest(ref ReadOnlySequence<byte> buffer, BinaryRequest into)
    {
        if (!UltraHardenedParser.TryExtractFullHeaderValidated(ref buffer, into, Limits, out var bytesRead))
        {
            return false;
        }

        buffer = buffer.Slice(bytesRead + 1);
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
}
