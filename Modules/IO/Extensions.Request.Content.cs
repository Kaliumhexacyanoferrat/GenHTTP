using System.Buffers;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.IO.Streaming;

namespace GenHTTP.Modules.IO;

public static class RequestContentExtensions
{

    public static async ValueTask<ReadOnlyMemory<byte>> ReadToEndAsync(this IRequestBody body)
    {
        var first = await body.TryReadAsync();

        if (first is null)
        {
            return ReadOnlyMemory<byte>.Empty;
        }

        var second = await body.TryReadAsync();

        if (second is null)
        {
            return first.Value;
        }

        var buffer = new ArrayBufferWriter<byte>(first.Value.Length * 2);
        buffer.Write(first.Value.Span);
        buffer.Write(second.Value.Span);

        ReadOnlyMemory<byte>? chunk;

        while ((chunk = await body.TryReadAsync()) is not null)
        {
            buffer.Write(chunk.Value.Span);
        }

        return buffer.WrittenMemory;
    }

    public static RequestContentStream AsStream(this IRequestBody body) => new RequestContentStream(body);

}
