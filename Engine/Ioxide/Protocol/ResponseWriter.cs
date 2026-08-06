using System.Buffers;
using System.IO.Pipelines;

using GenHTTP.Api.Protocol;

using GenHTTP.Engine.Shared.Types;

namespace GenHTTP.Engine.Ioxide.Protocol;

/// <summary>
/// Writes an <see cref="IResponse"/> to a <see cref="PipeWriter"/>. Status line and body writing
/// stay engine-specific (Ioxide sinks are allocated fresh per response rather than pooled on a
/// per-connection context), but header serialization is shared with the Internal engine via
/// <see cref="ResponseSerializer"/>; only the Server/Date header values differ.
/// </summary>
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
            // Unknown length: chunk-frame everything the content writes, then terminate.
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
