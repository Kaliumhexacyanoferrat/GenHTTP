using System.Buffers;
using System.IO.Pipelines;
using System.Net;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using ioxide.http2;

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

    // Maps one stream's request, and runs it through the dispatch HTTP/2 and HTTP/3 share.
    private static async ValueTask DispatchAsync(IServer server, IEndPoint endPoint, Http2Request request,
        Http2ResponseWriter writer, IPAddress? remoteAddress, bool secure)
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

            mapped.Apply(server, endPoint, request.Method, request.Path, request.Authority,
                reader is null ? null : reader.ReadAsync, remoteAddress, HttpProtocol.Http2, secure);

            await StreamedDriver.DispatchAsync(server, mapped, new Writer(writer), headRequest, "HTTP/2");
        }
        finally
        {
            if (mapped is not null)
            {
                StreamedRequest.Return(mapped);
            }
        }
    }

    /// <summary>ioxide's HTTP/2 response writer, as the shared dispatch drives it.</summary>
    private readonly struct Writer(Http2ResponseWriter writer) : IStreamedWriter
    {
        public IBufferWriter<byte> Body => writer;

        public Func<ValueTask<FlushResult>> Flush => writer.FlushAsync;

        public bool IsCompleted => writer.IsCompleted;

        public void WriteHead(StreamedResponseData head)
        {
            var response = new Http2Response { Status = head.Status };

            foreach ((ReadOnlyMemory<byte> name, ReadOnlyMemory<byte> value) in head.Headers)
            {
                response.Headers.Add(name, value);
            }

            writer.WriteHeaders(response);
        }

        public void WriteHead(int status) => writer.WriteHeaders(new Http2Response { Status = status });

        public ValueTask CompleteAsync() => writer.CompleteAsync();
    }
}
