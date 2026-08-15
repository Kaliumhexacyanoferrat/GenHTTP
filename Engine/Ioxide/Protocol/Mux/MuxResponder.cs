using System.Buffers;
using System.Buffers.Text;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Ioxide.Protocol.Mux;

/// <summary>
/// The status and fields of a response, ready to be handed to a protocol layer.
/// </summary>
/// <remarks>
/// Neutral on purpose. HTTP/2 and HTTP/3 want the same thing, but their response types come from
/// different packages, so each driver builds its own from this.
/// </remarks>
internal readonly struct MuxResponseData
{
    internal MuxResponseData(int status, List<(ReadOnlyMemory<byte> Name, ReadOnlyMemory<byte> Value)> headers)
    {
        Status = status;
        Headers = headers;
    }

    internal int Status { get; }

    internal List<(ReadOnlyMemory<byte> Name, ReadOnlyMemory<byte> Value)> Headers { get; }
}

/// <summary>
/// Maps a GenHTTP <see cref="IResponse"/> onto what a multiplexed protocol submits.
/// </summary>
internal static class MuxResponder
{
    private static readonly ReadOnlyMemory<byte> ContentTypeName = "content-type"u8.ToArray();

    private static readonly ReadOnlyMemory<byte> ContentEncodingName = "content-encoding"u8.ToArray();

    private static readonly ReadOnlyMemory<byte> ContentLengthName = "content-length"u8.ToArray();

    private static readonly ReadOnlyMemory<byte> ServerName = "server"u8.ToArray();

    private static readonly ReadOnlyMemory<byte> ServerValue = "ioxide-genhttp"u8.ToArray();

    /// <summary>
    /// Builds the field section. Does not touch the content, which is streamed afterwards.
    /// </summary>
    internal static MuxResponseData BuildHeaders(IResponse response)
    {
        var headers = new List<(ReadOnlyMemory<byte> Name, ReadOnlyMemory<byte> Value)>(response.Headers.Count + 4);

        for (var i = 0; i < response.Headers.Count; i++)
        {
            var header = response.Headers.GetMemoryEntry(i);

            // Connection-specific fields are malformed in HTTP/2 and HTTP/3 (RFC 9113 8.2.2,
            // RFC 9114 4.2) - a peer may treat one as a protocol error rather than ignore it.
            // Names are passed through as they are: both ioxide layers lowercase as they pack.
            if (!IsConnectionSpecific(header.Key.Span))
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

            // A streamed response has no length by the time its headers go out, so neither layer
            // fills this in - unlike their buffered paths, which know the body up front. Send it
            // when the content does know, which is every static file and every fixed page.
            if (content.Length is { } length)
            {
                headers.Add((ContentLengthName, Digits(length)));
            }
        }

        return new MuxResponseData((int)response.Status, headers);
    }

    /// <summary>
    /// Streams the content into the protocol's response writer.
    /// </summary>
    internal static async ValueTask WriteBodyAsync(IResponse response, IBufferWriter<byte> writer, Func<ValueTask> flush, bool headRequest)
    {
        var content = response.Content;

        if (content is null)
        {
            return;
        }

        try
        {
            // A HEAD response keeps the headers its GET would have produced and sends no body.
            if (!headRequest)
            {
                await content.WriteAsync(new MuxSink(writer, flush));
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

    private static ReadOnlyMemory<byte> Digits(ulong value)
    {
        var buffer = new byte[20];

        Utf8Formatter.TryFormat(value, buffer, out var written);

        return buffer.AsMemory(0, written);
    }

    private static bool IsConnectionSpecific(ReadOnlySpan<byte> name)
        => Matches(name, "connection"u8) || Matches(name, "keep-alive"u8) || Matches(name, "transfer-encoding"u8)
           || Matches(name, "upgrade"u8) || Matches(name, "proxy-connection"u8) || Matches(name, "server"u8)
           || Matches(name, "content-length"u8);

    private static bool Matches(ReadOnlySpan<byte> name, ReadOnlySpan<byte> lowercase)
    {
        if (name.Length != lowercase.Length)
        {
            return false;
        }

        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];

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
}
