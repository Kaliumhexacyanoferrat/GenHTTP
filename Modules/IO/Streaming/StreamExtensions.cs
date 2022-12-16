using System;
using System.Buffers;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using GenHTTP.Api.Content.Templating;

namespace GenHTTP.Modules.IO.Streaming
{

    public static class StreamExtensions
    {
        private static readonly ArrayPool<byte> POOL = ArrayPool<byte>.Shared;

        private static readonly Encoding UTF8 = Encoding.UTF8;

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

        public static async ValueTask RenderToStream<T>(this IRenderer<T> renderer, T model, Stream target) where T : class, IModel
        {
            var content = await renderer.RenderAsync(model);

            await content.WriteAsync(target);
        }

        public static async ValueTask WriteAsync(this string content, Stream target)
        {
            var encoder = UTF8.GetEncoder();

            var bytes = encoder.GetByteCount(content, false);

            var buffer = POOL.Rent(bytes);

            try
            {
                encoder.GetBytes(content.AsSpan(), buffer.AsSpan(), true);

                await target.WriteAsync(buffer.AsMemory(0, bytes));
            }
            finally
            {
                POOL.Return(buffer);
            }
        }

    }

}
