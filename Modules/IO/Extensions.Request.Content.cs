using System.Buffers;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO;

public static class RequestContentExtensions
{

    public static async ValueTask<ReadOnlyMemory<byte>> ReadToEndAsync(this IRequestBody body)
    {
        // todo: revisit, seems not efficient
        
        var input = body.AsStream();

        var buffer = new ArrayBufferWriter<byte>();

        var pool = ArrayPool<byte>.Shared;
        var pooledBuffer = pool.Rent(16384);

        try
        {
            int read;

            while ((read = await input.ReadAsync(pooledBuffer.AsMemory())) > 0)
            {
                buffer.Write(pooledBuffer.AsSpan(0, read));
            }
        }
        finally
        {
            pool.Return(pooledBuffer);
        }

        return buffer.WrittenMemory.ToArray();
    }

}
