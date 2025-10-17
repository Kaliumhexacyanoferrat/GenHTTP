using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.WiredIO.Hosting;

using Wired.IO.App;
using Wired.IO.Builder;
using Wired.IO.Http11;
using Wired.IO.Http11.Context;

namespace GenHTTP.Engine.WiredIO;

/// <summary>
/// Allows to create server instances.
/// </summary>
public static class Server
{

    /// <summary>
    /// Create a new, configurable server instance with
    /// default values.
    /// </summary>
    /// <param name="configurationHook">An action invoked with the pre-configured web application builder, allowing to customize the underlying ASP.NET app</param>
    /// <param name="applicationHook">An action invoked with the created application instance, allowing to customize the underlying ASP.NET app</param>
    /// <returns>The builder to create the instance</returns>
    public static IServerBuilder Create(Action<Builder<WiredHttp11, Http11Context>>? configurationHook = null, Action<WiredApp<Http11Context>>? applicationHook = null)
        => new WiredServerBuilder(configurationHook, applicationHook);

}
