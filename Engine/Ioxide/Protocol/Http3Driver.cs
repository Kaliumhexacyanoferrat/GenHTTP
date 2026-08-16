using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using ioxide;
using ioxide.nghttp3;

using Microsoft.Extensions.Logging;

using GenHTTP.Engine.Ioxide.Protocol.Multiplexed;

namespace GenHTTP.Engine.Ioxide.Protocol;

/// <summary>
/// Serves an HTTP/3 connection: ngtcp2 carries QUIC, nghttp3 carries HTTP/3 and QPACK, this maps
/// each request onto GenHTTP's handler chain.
/// </summary>
/// <remarks>
/// Streamed both ways, as with HTTP/2. A response is written as it is produced and each flush parks
/// until the peer's window and the connection's send-retention high-water allow more, so serving a
/// large file costs about that high-water in memory rather than the size of the file.
///
/// <para>nghttp3 brings the parts that are laborious by hand - QPACK with a static-table encoder,
/// stream priorities, GOAWAY draining - and ngtcp2 brings QUIC itself.</para>
/// </remarks>
internal static class Http3Driver
{
    private static readonly ReadOnlyMemory<byte> Head = "HEAD"u8.ToArray();

    /// <summary>
    /// Serves one accepted QUIC connection until it closes.
    /// </summary>
    internal static Task RunAsync(IServer server, IEndPoint endPoint, QuicConnection connection, Nghttp3Options options)
        => new Nghttp3Connection(connection, options)
            .RunStreamedResponseAsync((request, writer) => DispatchAsync(server, endPoint, request, writer));

    private static async ValueTask DispatchAsync(IServer server, IEndPoint endPoint, Nghttp3Request request,
        Nghttp3ResponseWriter writer)
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

            // Always secure: HTTP/3 runs over QUIC, which carries TLS 1.3 and has no cleartext mode.
            // The client address stays null - ioxide's QuicConnection tracks the peer address (it has
            // to, for path validation) but exposes no way to read it, and a QUIC peer may migrate
            // mid-connection anyway.
            await using var mapped = new MultiplexedRequest(server, endPoint, request.Method, request.Path, request.Authority,
                headers, reader is null ? null : reader.ReadAsync, remoteAddress: null, HttpProtocol.Http3, secure: true);

            var response = await server.Handler.HandleAsync(mapped)
                           ?? throw new InvalidOperationException("The root request handler did not return a response");

            var data = MultiplexedResponder.BuildHeaders(response);

            var head = new Nghttp3Response { Status = data.Status };

            foreach ((ReadOnlyMemory<byte> name, ReadOnlyMemory<byte> value) in data.Headers)
            {
                head.Headers.Add(name, value);
            }

            writer.WriteHeaders(head);

            await MultiplexedResponder.WriteBodyAsync(response, writer, writer.FlushAsync, headRequest);
        }
        catch (Exception e)
        {
            server.Logging.CreateLogger("GenHTTP.Engine.Ioxide.Protocol.Http3Driver")
                  .LogError(e, "Failed to handle HTTP/3 request");

            if (!writer.IsCompleted)
            {
                writer.WriteHeaders(new Nghttp3Response { Status = 500 });
            }
        }
        finally
        {
            // Ends the stream. Without it the peer waits for a body that is never coming.
            await writer.CompleteAsync();
        }
    }
}
