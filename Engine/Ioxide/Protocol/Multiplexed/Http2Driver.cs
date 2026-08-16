using System.IO.Pipelines;
using System.Net;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using ioxide.http2;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Engine.Ioxide.Protocol.Multiplexed;

/// <summary>
/// Serves an HTTP/2 connection: ioxide.http2 owns framing, HPACK and flow control, this maps each
/// request onto GenHTTP's handler chain.
/// </summary>
/// <remarks>
/// Streamed both ways: the handler starts at end-of-headers, pulls the body as flow control
/// delivers it, and writes into a writer that frames each flush as a DATA frame - so neither
/// direction is assembled in memory and both are paced by the peer's window.
/// </remarks>
internal static class Http2Driver
{
    private static readonly ReadOnlyMemory<byte> Head = "HEAD"u8.ToArray();

    private static readonly Http2Options Options = new() { StreamRequestBodies = true };

    /// <summary>
    /// Serves the connection over an established transport: a TLS pipe that negotiated "h2", or a
    /// plaintext pipe carrying h2c with prior knowledge.
    /// </summary>
    internal static Task RunAsync(IServer server, IEndPoint endPoint, IDuplexPipe pipe, IPAddress? remoteAddress, bool secure)
        => new Http2Connection(pipe, Options)
            .RunAsync((request, writer) => DispatchAsync(server, endPoint, request, writer, remoteAddress, secure));

    private static async ValueTask DispatchAsync(IServer server, IEndPoint endPoint, Http2Request request,
        Http2ResponseWriter writer, IPAddress? remoteAddress, bool secure)
    {
        try
        {
            var headers = new List<(ReadOnlyMemory<byte> Name, ReadOnlyMemory<byte> Value)>(request.Headers.Count);

            for (var i = 0; i < request.Headers.Count; i++)
            {
                var header = request.Headers[i];
                headers.Add((header.Key, header.Value));
            }

            var headRequest = request.Method.Span.SequenceEqual(Head.Span);

            var reader = request.BodyReader;

            await using var mapped = new MultiplexedRequest(server, endPoint, request.Method, request.Path, request.Authority,
                headers, reader is null ? null : reader.ReadAsync, remoteAddress, HttpProtocol.Http2, secure);

            var response = await server.Handler.HandleAsync(mapped)
                           ?? throw new InvalidOperationException("The root request handler did not return a response");

            var data = MultiplexedResponder.BuildHeaders(response);

            var head = new Http2Response { Status = data.Status };

            foreach ((ReadOnlyMemory<byte> name, ReadOnlyMemory<byte> value) in data.Headers)
            {
                head.Headers.Add(name, value);
            }

            writer.WriteHeaders(head);

            await MultiplexedResponder.WriteBodyAsync(response, writer, writer.FlushAsync, headRequest);
        }
        catch (Exception e)
        {
            server.Logging.CreateLogger("GenHTTP.Engine.Ioxide.Protocol.Http2Driver")
                  .LogError(e, "Failed to handle HTTP/2 request");

            if (!writer.IsCompleted)
            {
                writer.WriteHeaders(new Http2Response { Status = 500 });
            }
        }
        finally
        {
            // Ends the stream. Without it the peer waits for a body that is never coming.
            await writer.CompleteAsync();
        }
    }
}
