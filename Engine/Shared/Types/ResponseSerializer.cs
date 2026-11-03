using System.IO.Pipelines;
using System.Runtime.CompilerServices;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Shared.Types;

public static class ResponseSerializer
{
    private const int MaxNumberLength = 20;

    private static ReadOnlySpan<byte> Newline => "\r\n"u8;

    private static ReadOnlySpan<byte> HeaderSeparator => ": "u8;

    private static ReadOnlySpan<byte> ContentTypePrefix => "Content-Type: "u8;

    private static ReadOnlySpan<byte> ContentLengthPrefix => "Content-Length: "u8;

    private static ReadOnlySpan<byte> ContentEncodingPrefix => "Content-Encoding: "u8;

    private static ReadOnlySpan<byte> ChunkedHeader => "Transfer-Encoding: chunked\r\n"u8;

    private static ReadOnlySpan<byte> EmptyContentLength => "Content-Length: 0\r\n"u8;

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

    /// <summary>
    /// Writes the status line and the headers of the final response to the given request.
    /// </summary>
    /// <remarks>
    /// Calculates the size of the head first, so that it can be written into a single span
    /// instead of issuing a separate write for every fragment.
    /// </remarks>
    /// <param name="request">The request the response is written for, if any</param>
    public static void WriteHead(PipeWriter writer, Request? request, IResponse response, bool keepAlive, ReadOnlyMemory<byte> serverHeader, ReadOnlyMemory<byte> dateHeader, bool isHttp10)
    {
        request?.ResponseStarting();

        var headers = response.Headers;

        var statusLine = StatusLine.Get(response.Status);

        var server = headers.ContainsKey(KnownHeaders.Server) ? ReadOnlySpan<byte>.Empty : serverHeader.Span;
        var date = headers.ContainsKey(KnownHeaders.Date) ? ReadOnlySpan<byte>.Empty : dateHeader.Span;

        var isUpgrade = response.Mode == Connection.Upgrade;

        var connection = GetConnectionHeader(isUpgrade, keepAlive, isHttp10);

        var content = response.Content;

        var type = content?.Type;
        var length = content?.Length;
        var encoding = content?.Encoding;

        var size = statusLine.Length + server.Length + date.Length + connection.Length + Newline.Length;

        if (content != null)
        {
            if (type != null)
            {
                size += ContentTypePrefix.Length + type.Value.Bytes.Length + Newline.Length;
            }

            if (length != null)
            {
                size += ContentLengthPrefix.Length + MaxNumberLength + Newline.Length;
            }
            else if (!isUpgrade)
            {
                size += ChunkedHeader.Length;
            }

            if (encoding != null)
            {
                size += ContentEncodingPrefix.Length + encoding.Value.Length + Newline.Length;
            }
        }
        else
        {
            size += EmptyContentLength.Length;
        }

        var count = headers.Count;

        for (var i = 0; i < count; i++)
        {
            var header = headers.GetMemoryEntry(i);

            size += header.Key.Length + HeaderSeparator.Length + header.Value.Length + Newline.Length;
        }

        var span = writer.GetSpan(size);

        var offset = 0;

        Append(span, ref offset, statusLine);
        Append(span, ref offset, server);
        Append(span, ref offset, date);
        Append(span, ref offset, connection);

        if (content != null)
        {
            if (type != null)
            {
                Append(span, ref offset, ContentTypePrefix);
                Append(span, ref offset, type.Value.Bytes.Span);
                Append(span, ref offset, Newline);
            }

            if (length != null)
            {
                Append(span, ref offset, ContentLengthPrefix);

                if (!length.Value.TryFormat(span[offset..], out var written))
                {
                    throw new InvalidOperationException("Unable to write number to buffer");
                }

                offset += written;

                Append(span, ref offset, Newline);
            }
            else if (!isUpgrade)
            {
                Append(span, ref offset, ChunkedHeader);
            }

            if (encoding != null)
            {
                Append(span, ref offset, ContentEncodingPrefix);
                Append(span, ref offset, encoding.Value.Span);
                Append(span, ref offset, Newline);
            }
        }
        else
        {
            Append(span, ref offset, EmptyContentLength);
        }

        for (var i = 0; i < count; i++)
        {
            var header = headers.GetMemoryEntry(i);

            Append(span, ref offset, header.Key.Span);
            Append(span, ref offset, HeaderSeparator);
            Append(span, ref offset, header.Value.Span);
            Append(span, ref offset, Newline);
        }

        Append(span, ref offset, Newline);

        writer.Advance(offset);
    }

    private static ReadOnlySpan<byte> GetConnectionHeader(bool isUpgrade, bool keepAlive, bool isHttp10)
    {
        if (isUpgrade)
        {
            return "Connection: Upgrade\r\n"u8;
        }

        if (isHttp10)
        {
            return keepAlive ? "Connection: Keep-Alive\r\n"u8 : "Connection: Close\r\n"u8;
        }

        // HTTP/1.1 connections are persistent by default so we do not need to send a Keep-Alive header
        return keepAlive ? ReadOnlySpan<byte>.Empty : "Connection: Close\r\n"u8;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Append(Span<byte> target, ref int offset, ReadOnlySpan<byte> value)
    {
        value.CopyTo(target[offset..]);
        offset += value.Length;
    }

}
