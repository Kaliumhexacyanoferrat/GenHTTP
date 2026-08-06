using System.Buffers;
using System.IO.Pipelines;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Shared.Types;

public static class ResponseSerializer
{

    public static bool ShouldSendBody(IRequest? request, IResponse response, bool headRequest)
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

    public static void WriteHeader(PipeWriter writer, IResponse response, bool keepAlive, ReadOnlyMemory<byte> serverHeader, ReadOnlyMemory<byte> dateHeader, bool isHttp10)
    {
        var isUpgrade = response.Mode == Connection.Upgrade;

        if (!response.Headers.ContainsKey(KnownHeaders.Server))
        {
            writer.Write(serverHeader.Span);
        }

        if (!response.Headers.ContainsKey(KnownHeaders.Date))
        {
            writer.Write(dateHeader.Span);
        }

        if (isUpgrade)
        {
            writer.Write("Connection: Upgrade\r\n"u8);
        }
        else if (isHttp10)
        {
            writer.Write(keepAlive ? "Connection: Keep-Alive\r\n"u8 : "Connection: Close\r\n"u8);
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
                writer.Write(length.Value);
                writer.Write("\r\n"u8);
            }
            else if (!isUpgrade)
            {
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
            var header = headers.GetMemoryEntry(i);

            writer.Write(header.Key.Span);
            writer.Write(": "u8);
            writer.Write(header.Value.Span);
            writer.Write("\r\n"u8);
        }
    }

}
