using GenHTTP.Adapters.AspNetCore.Mapping;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace GenHTTP.Adapters.AspNetCore;

public static class Adapter
{

    public static void Map<T>(WebApplication app, PathString path, IServerCompanion? companion = null) where T : IHandler, new()
        => Map(app, path, new T(), companion);

    public static void Map(WebApplication app, PathString path, IHandlerBuilder handler, IServerCompanion? companion = null)
        => Map(app, path, handler.Build(), companion);

    public static void Map(WebApplication app, PathString path, IHandler handler, IServerCompanion? companion = null)
        => app.Map(path, async (context) => await Bridge.MapAsync(context, handler, companion: companion));

    public static void Map<T>(WebApplication app, string path, IServerCompanion? companion = null) where T : IHandler, new()
        => Map(app, path, new T(), companion);

    public static void Map(WebApplication app, string path, IHandlerBuilder handler, IServerCompanion? companion = null)
        => Map(app, path, handler.Build(), companion);

    public static void Map(WebApplication app, string path, IHandler handler, IServerCompanion? companion = null)
        => app.Map(path, async (context) => await Bridge.MapAsync(context, handler, companion: companion));

    public static void Run<T>(this IApplicationBuilder app, IServerCompanion? companion = null) where T : IHandler, new()
        => Run(app, new T(), companion: companion);

    public static void Run(this IApplicationBuilder app, IHandlerBuilder handler, IServerCompanion? companion = null)
        => Run(app, handler.Build(), companion: companion);

    public static void Run(this IApplicationBuilder app, IHandler handler, IServer? server = null, IServerCompanion? companion = null)
        => app.Run(async (context) => await Bridge.MapAsync(context, handler, server, companion));

}
