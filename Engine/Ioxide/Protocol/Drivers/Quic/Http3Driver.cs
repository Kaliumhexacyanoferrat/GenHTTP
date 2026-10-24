using System.Buffers;
using System.IO.Pipelines;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using ioxide;
using ioxide.nghttp3;

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

    // Maps one stream's request, and runs it through the dispatch HTTP/2 and HTTP/3 share.
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

            await StreamedDriver.DispatchAsync(server, mapped, new Writer(writer), headRequest, "HTTP/3");
        }
        finally
        {
            if (mapped is not null)
            {
                StreamedRequest.Return(mapped);
            }
        }
    }

    /// <summary>ioxide's HTTP/3 response writer, as the shared dispatch drives it.</summary>
    private readonly struct Writer(Nghttp3ResponseWriter writer) : IStreamedWriter
    {
        public IBufferWriter<byte> Body => writer;

        public Func<ValueTask<FlushResult>> Flush => writer.FlushAsync;

        public bool IsCompleted => writer.IsCompleted;

        public void WriteHead(StreamedResponseData head)
        {
            var response = new Nghttp3Response { Status = head.Status };

            foreach ((ReadOnlyMemory<byte> name, ReadOnlyMemory<byte> value) in head.Headers)
            {
                response.Headers.Add(name, value);
            }

            writer.WriteHeaders(response);
        }

        public void WriteHead(int status) => writer.WriteHeaders(new Nghttp3Response { Status = status });

        public ValueTask CompleteAsync() => writer.CompleteAsync();
    }

}
