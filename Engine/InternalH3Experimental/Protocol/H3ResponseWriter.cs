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

            // Names pass through as they are. HTTP/3 requires them lowercase, but Glyph3 resolves
            // static-table names case-insensitively - and lowercases the ones it has to write out -
            // so converting here only duplicated the work and allocated to do it.
            //
            // Connection-specific fields are malformed in HTTP/3 (RFC 9114 4.2), and a peer may
            // treat them as a protocol error rather than ignore them.
            if (!IsConnectionSpecific(header.Key.Span))
            {
                result.Headers.Add((header.Key, header.Value));
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
