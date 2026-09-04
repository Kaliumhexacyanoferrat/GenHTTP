using System.IO.Pipelines;
using System.Net;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using ioxide.http2;

using Microsoft.Extensions.Logging;
using GenHTTP.Engine.Ioxide.Protocol.Requests;
using GenHTTP.Engine.Ioxide.Protocol.Responses;

namespace GenHTTP.Engine.Ioxide.Protocol.Drivers.Tcp;

/// <summary>Serves HTTP/2 on one connection, a handler call per stream.</summary>
internal static class Http2Driver
{
    private static readonly ReadOnlyMemory<byte> Head = "HEAD"u8.ToArray();

    private static readonly Http2Options Options = new() { StreamRequestBodies = true };

    // Runs one HTTP/2 connection, dispatching each stream as it opens.
    internal static Task RunAsync(IServer server, IEndPoint endPoint, IDuplexPipe pipe, IPAddress? remoteAddress, bool secure)
        => new Http2Connection(pipe, Options)
            .RunAsync((request, writer) => DispatchAsync(server, endPoint, request, writer, remoteAddress, secure));

    // One stream through the handler chain; a fault becomes a 500 rather than a dead stream.
    private static async ValueTask DispatchAsync(IServer server, IEndPoint endPoint, Http2Request request,
        Http2ResponseWriter writer, IPAddress? remoteAddress, bool secure)
    {
        StreamedRequest? mapped = null;

        // Whether the head is already on the wire. IsCompleted cannot answer this - it means
        // END_STREAM has been sent - so a fault AFTER the headers went out used to take the error
        // path anyway, where WriteHeaders throws "headers have already been written". That second
        // exception replaced the first, so the only thing ever reported was the masking error and
        // the real fault was never logged at all.
        var headersWritten = false;

        try
        {
            var headRequest = request.Method.Span.SequenceEqual(Head.Span);

            var reader = request.BodyReader;

            mapped = StreamedRequest.Rent();

            for (var i = 0; i < request.Headers.Count; i++)
            {
                var header = request.Headers[i];
                mapped.AddHeader(header.Key, header.Value);
            }

            mapped.Apply(server, endPoint, request.Method, request.Path, request.Authority,
                reader is null ? null : reader.ReadAsync, remoteAddress, HttpProtocol.Http2, secure);

            var response = await server.Handler.HandleAsync(mapped)
                           ?? throw new InvalidOperationException("The root request handler did not return a response");

            var data = StreamedResponder.BuildHeaders(response, mapped);

            var head = new Http2Response { Status = data.Status };

            foreach ((ReadOnlyMemory<byte> name, ReadOnlyMemory<byte> value) in data.Headers)
            {
                head.Headers.Add(name, value);
            }

            writer.WriteHeaders(head);
            headersWritten = true;

            await StreamedResponder.WriteBodyAsync(response, writer, writer.FlushAsync, headRequest);
        }
        catch (Exception e)
        {
            server.Logging.CreateLogger("GenHTTP.Engine.Ioxide.Protocol.Drivers.Tcp.Http2Driver")
                  .LogError(e, "Failed to handle HTTP/2 request");

            if (!headersWritten && !writer.IsCompleted)
            {
                writer.WriteHeaders(new Http2Response { Status = 500 });
            }
        }
        finally
        {
            if (mapped is not null)
            {
                StreamedRequest.Return(mapped);
            }

            await writer.CompleteAsync();
        }
    }
}
