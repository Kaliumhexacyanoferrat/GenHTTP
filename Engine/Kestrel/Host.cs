using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Kestrel.Hosting;
using Microsoft.AspNetCore.Builder;

namespace GenHTTP.Engine.Kestrel;

public static class Host
{

    /// <summary>
    /// Provides a new server host that runs the GenHTTP webserver on Kestrel.
    /// </summary>
    /// <param name="configHook">Invoked to customize the web application builder</param>
    /// <param name="appHook">Invoked to customize the web application</param>
    public static IServerHost Create(Action<WebApplicationBuilder>? configHook = null, Action<WebApplication>? appHook = null) => new KestrelServerHost(configHook, appHook);

}
