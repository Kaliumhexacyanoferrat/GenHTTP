using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Kestrel.Hosting;
using Microsoft.AspNetCore.Builder;

namespace GenHTTP.Engine.Kestrel;

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
    public static IServerBuilder Create(Action<WebApplicationBuilder>? configurationHook = null, Action<WebApplication>? applicationHook = null)
        => new KestrelServerBuilder(configurationHook, applicationHook);

}
