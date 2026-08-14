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
        ReadOnlyMemory<byte> body = default;

        IResponseContent? content = response.Content;

        // A HEAD response keeps the headers its GET would have produced and sends no body.
        if (content is not null && !headRequest)
        {
            var buffer = new ArrayBufferWriter<byte>(
                content.Length is { } length and > 0 and < int.MaxValue ? (int)length : 4096);

            await content.WriteAsync(new H3Sink(buffer));

            body = buffer.WrittenMemory;
        }

        // Written straight into the response. Collecting into a list first and copying it across
        // allocated a second list and its backing array on every single response.
        var result = new Http3Response
        {
            Status = (int)response.Status,
            Body = body,
        };

        for (int i = 0; i < response.Headers.Count; i++)
        {
            KeyValuePair<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>> header = response.Headers.GetMemoryEntry(i);

            // Connection-specific fields are malformed in HTTP/3 (RFC 9114 4.2), and a peer may
            // treat them as a protocol error rather than ignore them.
            if (!IsConnectionSpecific(header.Key.Span))
            {
                result.Headers.Add((Lowercase(header.Key), header.Value));
            }
        }

        if (content is not null)
        {
            if (content.Type is { } type)
            {
                result.Headers.Add((ContentTypeName, type.Bytes));
            }

            if (content.Encoding is { } encoding)
            {
                result.Headers.Add((ContentEncodingName, encoding));
            }
        }

        return result;
    }

    // The field names GenHTTP actually emits, pre-lowercased. Every one of these arrives with
    // capitals, so without this table each of them allocated a fresh array on every response.
    private static readonly byte[][] KnownNames =
    [
        "server"u8.ToArray(), "date"u8.ToArray(), "content-type"u8.ToArray(),
        "content-encoding"u8.ToArray(), "content-disposition"u8.ToArray(), "content-range"u8.ToArray(),
        "cache-control"u8.ToArray(), "last-modified"u8.ToArray(), "expires"u8.ToArray(),
        "location"u8.ToArray(), "etag"u8.ToArray(), "vary"u8.ToArray(),
        "accept-ranges"u8.ToArray(), "set-cookie"u8.ToArray(), "alt-svc"u8.ToArray(),
        "access-control-allow-origin"u8.ToArray(), "www-authenticate"u8.ToArray(),
    ];

    // HTTP/3 requires lowercase field names; anything else is a malformed message.
    private static ReadOnlyMemory<byte> Lowercase(ReadOnlyMemory<byte> name)
    {
        ReadOnlySpan<byte> span = name.Span;

        for (int i = 0; i < span.Length; i++)
        {
            if (span[i] is >= (byte)'A' and <= (byte)'Z')
            {
                // Matches compares case-insensitively, so a known name resolves to a shared array.
                foreach (byte[] known in KnownNames)
                {
                    if (Matches(span, known))
                    {
                        return known;
                    }
                }

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
