using System.Buffers;

using GenHTTP.Api.Protocol;

using Glyph3;

namespace GenHTTP.Engine.InternalH3Experimental.Protocol;

/// <summary>
/// Turns a GenHTTP <see cref="IResponse"/> into a Glyph3 response.
/// </summary>
internal static class H3ResponseWriter
{

    internal static async ValueTask<Http3Response> BuildAsync(IResponse response, bool headRequest)
    {
        var headers = new List<(ReadOnlyMemory<byte> Name, ReadOnlyMemory<byte> Value)>();

        for (int i = 0; i < response.Headers.Count; i++)
        {
            KeyValuePair<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>> header = response.Headers.GetMemoryEntry(i);

            // Connection-specific fields are malformed in HTTP/3 (RFC 9114 4.2), and a peer may
            // treat them as a protocol error rather than ignore them.
            if (!IsConnectionSpecific(header.Key.Span))
            {
                headers.Add((Lowercase(header.Key), header.Value));
            }
        }

        ReadOnlyMemory<byte> body = default;

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

            // A HEAD response keeps the headers its GET would have produced and sends no body.
            if (!headRequest)
            {
                var buffer = new ArrayBufferWriter<byte>(
                    content.Length is { } length and > 0 and < int.MaxValue ? (int)length : 4096);

                await content.WriteAsync(new H3Sink(buffer));

                body = buffer.WrittenMemory;
            }
        }

        var result = new Http3Response
        {
            Status = (int)response.Status,
            Body = body,
        };

        foreach ((ReadOnlyMemory<byte> name, ReadOnlyMemory<byte> value) in headers)
        {
            result.Headers.Add((name, value));
        }

        return result;
    }

    // HTTP/3 requires lowercase field names; anything else is a malformed message.
    private static ReadOnlyMemory<byte> Lowercase(ReadOnlyMemory<byte> name)
    {
        ReadOnlySpan<byte> span = name.Span;

        for (int i = 0; i < span.Length; i++)
        {
            if (span[i] is >= (byte)'A' and <= (byte)'Z')
            {
                byte[] lowered = name.ToArray();
                for (int j = 0; j < lowered.Length; j++)
                {
                    if (lowered[j] is >= (byte)'A' and <= (byte)'Z')
                    {
                        lowered[j] += 32;
                    }
                }
                return lowered;
            }
        }

        return name;
    }

    private static bool IsConnectionSpecific(ReadOnlySpan<byte> name)
        => Matches(name, "connection"u8) || Matches(name, "keep-alive"u8) || Matches(name, "transfer-encoding"u8)
        || Matches(name, "upgrade"u8) || Matches(name, "proxy-connection"u8) || Matches(name, "content-length"u8);

    private static bool Matches(ReadOnlySpan<byte> name, ReadOnlySpan<byte> lowercase)
    {
        if (name.Length != lowercase.Length)
        {
            return false;
        }

        for (int i = 0; i < name.Length; i++)
        {
            byte c = name[i];
            if (c is >= (byte)'A' and <= (byte)'Z')
            {
                c += 32;
            }
            if (c != lowercase[i])
            {
                return false;
            }
        }

        return true;
    }

    private static readonly ReadOnlyMemory<byte> ContentTypeName = "content-type"u8.ToArray();
    private static readonly ReadOnlyMemory<byte> ContentEncodingName = "content-encoding"u8.ToArray();
}
