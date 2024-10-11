using System.Buffers;
using System.Text;

namespace GenHTTP.Modules.IO.Streaming;

public static class StreamExtensions
{
    private static readonly ArrayPool<byte> POOL = ArrayPool<byte>.Shared;

    private static readonly Encoding UTF8 = Encoding.UTF8;

    private static readonly Encoder ENCODER = UTF8.GetEncoder();

    public static async ValueTask CopyPooledAsync(this Stream source, Stream target, uint bufferSize)
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

    public static async ValueTask WriteAsync(this string content, Stream target)
    {
            var bytes = ENCODER.GetByteCount(content, false);

            var buffer = POOL.Rent(bytes);

            try
            {
                ENCODER.GetBytes(content.AsSpan(), buffer.AsSpan(), true);

                await target.WriteAsync(buffer.AsMemory(0, bytes));
            }
            finally
            {
                POOL.Return(buffer);
            }
        }

    public static void Write(this string content, Stream target)
    {
            var length = ENCODER.GetByteCount(content, false);

            var buffer = POOL.Rent(length);

            try
            {
                ENCODER.GetBytes(content.AsSpan(), buffer.AsSpan(), true);

                target.Write(buffer, 0, length);
            }
            finally
            {
                POOL.Return(buffer);
            }
        }

    /// <summary>
    /// Efficiently calculates the checksum of the stream, beginning
    /// from the current position. Resets the position to the previous
    /// one.
    /// </summary>
    /// <returns>The checksum of the stream</returns>
    public static async ValueTask<ulong?> CalculateChecksumAsync(this Stream stream)
    {
            if (stream.CanSeek)
            {
                var position = stream.Position;

                try
                {
                    unchecked
                    {
                        ulong hash = 17;

                        var buffer = POOL.Rent(4096);

                        try
                        {
                            var read = 0;

                            do
                            {
                                read = await stream.ReadAsync(buffer);

                                for (int i = 0; i < read; i++)
                                {
                                    hash = hash * 23 + buffer[i];
                                }
                            }
                            while (read > 0);
                        }
                        finally
                        {
                            POOL.Return(buffer);
                        }

                        return hash;
                    }
                }
                finally
                {
                    stream.Seek(position, SeekOrigin.Begin);
                }
            }

            return null;
        }

}
