using GenHTTP.Api.Content;

using GenHTTP.Benchmarks.Infrastructure.Context;

using GenHTTP.Engine.Internal.Protocol;
using GenHTTP.Engine.Internal.Utilities;

namespace GenHTTP.Benchmarks.Infrastructure;

public abstract class HandlerBenchmark
{
    private const uint BufferSize = 65 * 1024;

    protected IHandler? Handler { get; set; }

    protected BenchmarkContext? Context { get; set; }

    protected async ValueTask Run(bool withContent)
    {
        var handler = Handler!;
        var context = Context!;

        var response = await handler.HandleAsync(context.Request);

        if ((response != null) && withContent)
        {
            var content = response.Content;

            if (content != null)
            {
                using var target = new MemoryStream();

                await using var buffer = new PoolBufferedStream(target, BufferSize);

                if (response.ContentLength == null)
                {
                    await using var chunked = new ChunkedStream(buffer);

                    await content.WriteAsync(chunked, BufferSize);

                    chunked.Finish();
                }
                else
                {
                    await content.WriteAsync(buffer, BufferSize);
                }
            }
        }

        context.Reset();
    }

}
