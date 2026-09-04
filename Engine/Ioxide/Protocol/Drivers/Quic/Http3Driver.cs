using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using ioxide;
using ioxide.nghttp3;

using Microsoft.Extensions.Logging;
using GenHTTP.Engine.Ioxide.Protocol.Requests;
using GenHTTP.Engine.Ioxide.Protocol.Responses;

namespace GenHTTP.Engine.Ioxide.Protocol.Drivers.Quic;

/// <summary>Serves HTTP/3 on one QUIC connection, a handler call per stream.</summary>
internal static class Http3Driver
{
    private static readonly ReadOnlyMemory<byte> Head = "HEAD"u8.ToArray();

    // Runs one QUIC connection as HTTP/3, dispatching each stream as it opens.
    internal static Task RunAsync(IServer server, IEndPoint endPoint, QuicConnection connection, Nghttp3Options options)
        => new Nghttp3Connection(connection, options)
            .RunStreamedResponseAsync((request, writer) => DispatchAsync(server, endPoint, request, writer));

    // One stream through the handler chain; a fault becomes a 500 rather than a dead stream.
    private static async ValueTask DispatchAsync(IServer server, IEndPoint endPoint, Nghttp3Request request,
        Nghttp3ResponseWriter writer)
    {
        StreamedRequest? mapped = null;

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

            // No remote address: ioxide's QuicConnection exposes none, and a QUIC peer may migrate
            // mid-connection anyway.
            mapped.Apply(server, endPoint, request.Method, request.Path, request.Authority,
                reader is null ? null : reader.ReadAsync, remoteAddress: null, HttpProtocol.Http3, secure: true);

            var response = await server.Handler.HandleAsync(mapped)
                           ?? throw new InvalidOperationException("The root request handler did not return a response");

            var data = StreamedResponder.BuildHeaders(response, mapped);

            var head = new Nghttp3Response { Status = data.Status };

            foreach ((ReadOnlyMemory<byte> name, ReadOnlyMemory<byte> value) in data.Headers)
            {
                head.Headers.Add(name, value);
            }

            writer.WriteHeaders(head);

            await StreamedResponder.WriteBodyAsync(response, writer, writer.FlushAsync, headRequest);
        }
        catch (Exception e)
        {
            server.Logging.CreateLogger("GenHTTP.Engine.Ioxide.Protocol.Drivers.Quic.Http3Driver")
                  .LogError(e, "Failed to handle HTTP/3 request");

            if (!writer.IsCompleted)
            {
                writer.WriteHeaders(new Nghttp3Response { Status = 500 });
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
