using System;
using System.Buffers;
using System.IO;

using PooledAwait;

namespace GenHTTP.Modules.IO.Streaming
{

    public static class StreamExtensions
    {
        private static readonly ArrayPool<byte> POOL = ArrayPool<byte>.Shared;

        public static async PooledValueTask CopyPooledAsync(this Stream source, Stream target, uint bufferSize)
        {
            if (source.CanSeek && source.Position != 0)
            {
                source.Seek(0, SeekOrigin.Begin);
            }

            var buffer = POOL.Rent((int)bufferSize);

            var memory = buffer.AsMemory();

            try
            {
                int read;

                do
                {
                    read = await source.ReadAsync(memory);

                    if (read > 0)
                    {
                        await target.WriteAsync(memory[..read]);
                    }
                }
                while (read > 0);
            }
            finally
            {
                POOL.Return(buffer);
            }
        }

    }

}
