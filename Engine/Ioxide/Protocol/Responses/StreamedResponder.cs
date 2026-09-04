using System.Buffers;
using GenHTTP.Api.Protocol;
using GenHTTP.Engine.Ioxide.Protocol.Requests;
using GenHTTP.Engine.Ioxide.Protocol.Sinks;

namespace GenHTTP.Engine.Ioxide.Protocol.Responses;

/// <summary>A built response head, on its way to whichever driver sends it.</summary>
internal readonly struct StreamedResponseData
{
    
    // Carries a built response head between the responder and whichever driver sends it.
    internal StreamedResponseData(int status, List<(ReadOnlyMemory<byte> Name, ReadOnlyMemory<byte> Value)> headers)
    {
        Status = status;
        Headers = headers;
    }

    internal int Status { get; }

    internal List<(ReadOnlyMemory<byte> Name, ReadOnlyMemory<byte> Value)> Headers { get; }
    
}

/// <summary>The response shaping HTTP/2 and HTTP/3 have in common.</summary>
internal static class StreamedResponder
{
    private static readonly ReadOnlyMemory<byte> ContentTypeName = "content-type"u8.ToArray();

    private static readonly ReadOnlyMemory<byte> ContentEncodingName = "content-encoding"u8.ToArray();

    private static readonly ReadOnlyMemory<byte> ContentLengthName = "content-length"u8.ToArray();

    private static readonly ReadOnlyMemory<byte> ServerName = "server"u8.ToArray();

    private static readonly ReadOnlyMemory<byte> ServerValue = "ioxide-genhttp"u8.ToArray();

    // The headers HTTP/2 and HTTP/3 treat as malformed, so they must not be forwarded (RFC 9113
    // 8.2.2, RFC 9114 4.2). Held as ByteStrings, which compare against a header name case-insensitively.
    private static readonly ByteString[] ConnectionSpecific =
    [
        new("connection"),
        new("keep-alive"),
        new("transfer-encoding"),
        new("upgrade"),
        new("proxy-connection"),
        new("server"),
        new("content-length"),
    ];

    // The response head as both protocols want it: connection-specific fields dropped, content fields
    // filled in. Built into the exchange's own list and content-length buffer, so a response with a
    // known length allocates nothing here.
    internal static StreamedResponseData BuildHeaders(IResponse response, StreamedRequest exchange)
    {
        var headers = exchange.ResponseHeaders;

        for (var i = 0; i < response.Headers.Count; i++)
        {
            var header = response.Headers.GetMemoryEntry(i);

            if (!IsConnectionSpecific(header.Key))
            {
                headers.Add((header.Key, header.Value));
            }
        }

        headers.Add((ServerName, ServerValue));

        if (response.Content is { } content)
        {
            if (content.Type is { } type)
            {
                headers.Add((ContentTypeName, type.Bytes));
            }

            if (content.Encoding is { } encoding)
            {
                headers.Add((ContentEncodingName, encoding));
            }

            if (content.Length is { } length)
            {
                headers.Add((ContentLengthName, exchange.Digits(length)));
            }
        }

        return new StreamedResponseData((int)response.Status, headers);
    }

    // Writes the content, or skips it for a HEAD while still disposing it.
    internal static async ValueTask WriteBodyAsync(IResponse response, IBufferWriter<byte> writer, Func<ValueTask> flush, bool headRequest)
    {
        var content = response.Content;

        if (content is null)
        {
            return;
        }

        try
        {
            if (!headRequest)
            {
                await content.WriteAsync(new StreamedSink(writer, flush));
            }
        }
        finally
        {
            if (content is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    // Whether a header is one HTTP/2 and HTTP/3 treat as malformed (RFC 9113 8.2.2, RFC 9114 4.2).
    private static bool IsConnectionSpecific(ReadOnlyMemory<byte> name)
    {
        for (var i = 0; i < ConnectionSpecific.Length; i++)
        {
            if (ConnectionSpecific[i] == name)
            {
                return true;
            }
        }

        return false;
    }

}
