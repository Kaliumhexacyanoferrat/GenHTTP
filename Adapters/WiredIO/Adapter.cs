using GenHTTP.Adapters.WiredIO.Mapping;
using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.ClientCaching;
using GenHTTP.Modules.Compression;
using GenHTTP.Modules.ErrorHandling;
using GenHTTP.Modules.IO;

using Wired.IO.Builder;
using Wired.IO.Http11Express;
using Wired.IO.Http11Express.Context;

namespace GenHTTP.Adapters.WiredIO;

public static class Adapter
{

    // ToDo: IBaseRequest and IBaseResponse do not feature basic access (such as headers), so we cannot be generic here

    public static void Map(this Builder<WiredHttp11Express<Http11ExpressContext>, Http11ExpressContext> builder, string path, IHandlerBuilder handler, IServerCompanion? companion = null)
        => Map(builder, path, handler.Build(), companion);

    public static void Map(this Builder<WiredHttp11Express<Http11ExpressContext>, Http11ExpressContext> builder, string path, IHandler handler, IServerCompanion? companion = null)
    {
        builder.UseMiddleware(scope => async (c, n) => await Bridge.MapAsync(c, n, handler, companion: companion, registeredPath: path));
    }

    /// <summary>
    /// Enables default features on the given handler. This should be used on the
    /// outer-most handler only.
    /// </summary>
    /// <param name="builder">The handler to be configured</param>
    /// <param name="errorHandling">If enabled, any exception will be catched and converted into an error response</param>
    /// <param name="compression">If enabled, responses will automatically be compressed if possible</param>
    /// <param name="clientCaching">If enabled, ETags are attached to any generated response and the tag is evaluated on the next request of the same resource</param>
    /// <param name="rangeSupport">If enabled, clients can request ranges instead of the complete response body</param>
    /// <typeparam name="T">The type of the handler builder which will be returned to allow the factory pattern</typeparam>
    /// <returns>The handler builder instance to be chained</returns>
    public static T Defaults<T>(this T builder, bool errorHandling = true, bool compression = true, bool clientCaching = true, bool rangeSupport = false) where T : IHandlerBuilder<T>
    {
        if (compression)
        {
            builder.Add(CompressedContent.Default());
        }

        if (rangeSupport)
        {
            builder.Add(RangeSupport.Create());
        }

        if (clientCaching)
        {
            builder.Add(ClientCache.Validation());
        }

        if (errorHandling)
        {
            builder.Add(ErrorHandler.Default());
        }

        return builder;
    }

}
