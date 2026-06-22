using System.Buffers;
using System.Buffers.Text;
using System.IO.Pipelines;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Ioxide.Protocol;

/// <summary>
/// Writes an <see cref="IResponse"/> to a <see cref="PipeWriter"/>. Forked from GenHTTP's
/// ResponseHandler; emits its own Server/Date headers. Fixed-length content is sent with a
/// Content-Length; unknown-length content is chunk-encoded (see <see cref="ChunkedWriter"/>).
/// </summary>
internal static class ResponseWriter
{
    private static readonly byte[] ServerHeader = "Server: ioxide-genhttp\r\n"u8.ToArray();

    internal static async ValueTask WriteAsync(PipeWriter writer, IRequest? request, IResponse response, bool keepAlive, bool headRequest)
    {
        writer.Write(StatusLine.Get(response.Status));

        WriteHeader(writer, response, keepAlive);

        writer.Write("\r\n"u8);

        if (ShouldSendBody(request, response, headRequest))
        {
            await WriteBodyAsync(writer, response);
        }
    }

    private static bool ShouldSendBody(IRequest? request, IResponse response, bool headRequest)
    {
        if (request == null)
        {
            return true;
        }

        if (headRequest)
        {
            return false;
        }

        var content = response.Content;

        if (content != null)
        {
            return (content.Length ?? 1) > 0;
        }

        return false;
    }

    private static void WriteHeader(PipeWriter writer, IResponse response, bool keepAlive)
    {
        var isUpgrade = response.Mode == Connection.Upgrade;

        if (!response.Headers.ContainsKey(KnownHeaders.Server))
        {
            writer.Write(ServerHeader);
        }

        if (!response.Headers.ContainsKey(KnownHeaders.Date))
        {
            writer.Write(DateHeader.Get());
        }

        if (isUpgrade)
        {
            writer.Write("Connection: Upgrade\r\n"u8);
        }
        else if (!keepAlive)
        {
            // HTTP/1.1 connections are persistent by default so we do not need to send a Keep-Alive header
            writer.Write("Connection: Close\r\n"u8);
        }

        var content = response.Content;

        if (content != null)
        {
            var type = content.Type;

            if (type != null)
            {
                writer.Write("Content-Type: "u8);
                writer.Write(type.Value.Bytes.Span);
                writer.Write("\r\n"u8);
            }

            var length = content.Length;

            if (length != null)
            {
                writer.Write("Content-Length: "u8);
                WriteNumber(writer, length.Value);
                writer.Write("\r\n"u8);
            }
            else if (!isUpgrade)
            {
                // Unknown length: chunk-encode the body (keep-alive stays intact).
                writer.Write("Transfer-Encoding: chunked\r\n"u8);
            }

            var encoding = content.Encoding;

            if (encoding != null)
            {
                writer.Write("Content-Encoding: "u8);
                writer.Write(encoding.Value.Span);
                writer.Write("\r\n"u8);
            }
        }
        else
        {
            writer.Write("Content-Length: 0\r\n"u8);
        }

        var headers = response.Headers;

        for (var i = 0; i < headers.Count; i++)
        {
            var header = headers[i];

            writer.Write(header.Key.Bytes.Span);
            writer.Write(": "u8);
            writer.Write(header.Value.Bytes.Span);
            writer.Write("\r\n"u8);
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

    private static void WriteNumber(PipeWriter writer, ulong value)
    {
        var span = writer.GetSpan(20);
        Utf8Formatter.TryFormat(value, span, out var written);
        writer.Advance(written);
    }
}
