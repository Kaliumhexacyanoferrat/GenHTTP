using System.Buffers;
using System.Buffers.Text;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Ioxide.Protocol.Multiplexed;

internal readonly struct MultiplexedResponseData
{
    internal MultiplexedResponseData(int status, List<(ReadOnlyMemory<byte> Name, ReadOnlyMemory<byte> Value)> headers)
    {
        Status = status;
        Headers = headers;
    }

    internal int Status { get; }

    internal List<(ReadOnlyMemory<byte> Name, ReadOnlyMemory<byte> Value)> Headers { get; }
}

internal static class MultiplexedResponder
{
    private static readonly ReadOnlyMemory<byte> ContentTypeName = "content-type"u8.ToArray();

    private static readonly ReadOnlyMemory<byte> ContentEncodingName = "content-encoding"u8.ToArray();

    private static readonly ReadOnlyMemory<byte> ContentLengthName = "content-length"u8.ToArray();

    private static readonly ReadOnlyMemory<byte> ServerName = "server"u8.ToArray();

    private static readonly ReadOnlyMemory<byte> ServerValue = "ioxide-genhttp"u8.ToArray();

    internal static MultiplexedResponseData BuildHeaders(IResponse response)
    {
        var headers = new List<(ReadOnlyMemory<byte> Name, ReadOnlyMemory<byte> Value)>(response.Headers.Count + 4);

        for (var i = 0; i < response.Headers.Count; i++)
        {
            var header = response.Headers.GetMemoryEntry(i);

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

            if (content.Length is { } length)
            {
                headers.Add((ContentLengthName, Digits(length)));
            }
        }

        return new MultiplexedResponseData((int)response.Status, headers);
    }

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
                await content.WriteAsync(new MultiplexedSink(writer, flush));
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
