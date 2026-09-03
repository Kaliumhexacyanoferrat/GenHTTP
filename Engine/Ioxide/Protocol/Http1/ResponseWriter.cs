using System.Buffers;
using System.IO.Pipelines;

using GenHTTP.Api.Protocol;

using GenHTTP.Engine.Shared.Types;

namespace GenHTTP.Engine.Ioxide.Protocol.Http1;

internal static class ResponseWriter
{
    private static readonly byte[] ServerHeader = "Server: ioxide-genhttp\r\n"u8.ToArray();

    internal static async ValueTask WriteAsync(PipeWriter writer, IRequest? request, IResponse response, bool keepAlive, bool headRequest)
    {
        writer.Write(StatusLine.Get(response.Status));

        ResponseSerializer.WriteHeader(writer, response, keepAlive, ServerHeader, DateHeader.Get(), isHttp10: false);

        writer.Write("\r\n"u8);

        if (ResponseSerializer.ShouldSendBody(request, response, headRequest))
        {
            await WriteBodyAsync(writer, response);
        }
    }

    private static async ValueTask WriteBodyAsync(PipeWriter writer, IResponse response)
    {
        var content = response.Content;

        if (content is null)
        {
            return;
        }

        if (content.Length is null && response.Mode != Connection.Upgrade)
        {
            var sink = new ChunkedSink(writer);
            await content.WriteAsync(sink);
            sink.Finish();
        }
        else
        {
            await content.WriteAsync(new IoxideSink(writer));
        }

        if (content is IDisposable disposableContent)
        {
            disposableContent.Dispose();
        }
    }
}
