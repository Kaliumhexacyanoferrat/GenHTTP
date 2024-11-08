using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Shared.Hosting;

using Microsoft.AspNetCore.Builder;

namespace GenHTTP.Engine.Kestrel;

public static class Host
{

    /// <summary>
    /// Provides a new server host that can be used to run a
    /// server instance of the GenHTTP webserver.
    /// </summary>
    /// <param name="configurationHook">An action invoked with the pre-configured web application builder, allowing to customize the underlying ASP.NET app</param>
    /// <param name="applicationHook">An action invoked with the created application instance, allowing to customize the underlying ASP.NET app</param>
    /// <returns>The host which can be used to run a server instance</returns>
    public static IServerHost Create(Action<WebApplicationBuilder>? configurationHook = null, Action<WebApplication>? applicationHook = null)
        => new ServerHost(Server.Create(configurationHook, applicationHook));

}
