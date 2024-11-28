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

    public static void Map<T>(this WebApplication app, string path, IServerCompanion? companion = null) where T : IHandler, new()
        => Map(app, path, new T(), companion);

    public static void Map(this WebApplication app, string path, IHandlerBuilder handler, IServerCompanion? companion = null)
        => Map(app, path, handler.Build(), companion);

    public static void Map(this WebApplication app, string path, IHandler handler, IServerCompanion? companion = null)
        => app.Map(path + "/{*any}", async (context) => await Bridge.MapAsync(context, handler, companion: companion, registeredPath: path));

    public static void Run<T>(this IApplicationBuilder app, IServerCompanion? companion = null) where T : IHandler, new()
        => Run(app, new T(), companion: companion);

    public static void Run(this IApplicationBuilder app, IHandlerBuilder handler, IServerCompanion? companion = null)
        => Run(app, handler.Build(), companion: companion);

    public static void Run(this IApplicationBuilder app, IHandler handler, IServer? server = null, IServerCompanion? companion = null)
        => app.Run(async (context) => await Bridge.MapAsync(context, handler, server, companion));

    public static IHandlerBuilder<T> Defaults<T>(this IHandlerBuilder<T> builder, bool errorHandling = true, bool compression = true, bool clientCaching = true, bool rangeSupport = false) where T : IHandlerBuilder<T>
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
