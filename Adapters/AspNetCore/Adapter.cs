using GenHTTP.Adapters.AspNetCore.Mapping;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.ClientCaching;
using GenHTTP.Modules.Compression;
using GenHTTP.Modules.ErrorHandling;
using GenHTTP.Modules.IO;

using Microsoft.AspNetCore.Builder;

namespace GenHTTP.Adapters.AspNetCore;

public static class Adapter
{

    /// <summary>
    /// Registers the given handler to respond to requests to the specified path.
    /// </summary>
    /// <param name="app">The application to add the mapping to</param>
    /// <param name="path">The path to register the handler for</param>
    /// <param name="handler">The handler to be registered</param>
    /// <param name="companion">An object that will be informed about handled requests and any error</param>
    public static void Map(this WebApplication app, string path, IHandlerBuilder handler, IServerCompanion? companion = null)
        => Map(app, path, handler.Build(), companion);

    /// <summary>
    /// Registers the given handler to respond to requests to the specified path.
    /// </summary>
    /// <param name="app">The application to add the mapping to</param>
    /// <param name="path">The path to register the handler for</param>
    /// <param name="handler">The handler to be registered</param>
    /// <param name="companion">An object that will be informed about handled requests and any error</param>
    public static void Map(this WebApplication app, string path, IHandler handler, IServerCompanion? companion = null)
        => app.Map(path + "/{*any}", async context => await Bridge.MapAsync(context, handler, companion: companion, registeredPath: path));

    /// <summary>
    /// Registers the given handler to respond to any request.
    /// </summary>
    /// <param name="app">The application to be configured</param>
    /// <param name="handler">The handler to be registered</param>
    /// <param name="server">The server instance that would like to execute requests</param>
    public static void Run(this IApplicationBuilder app, IHandler handler, IServer server)
        => app.Run(async context => await Bridge.MapAsync(context, handler, server));

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
