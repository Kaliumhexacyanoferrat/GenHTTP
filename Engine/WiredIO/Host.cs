using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Shared.Hosting;
using Wired.IO.App;
using Wired.IO.Builder;
using Wired.IO.Http11;
using Wired.IO.Http11.Context;

namespace GenHTTP.Engine.WiredIO;

public static class Host
{

    /// <summary>
    /// Provides a new server host that can be used to run a
    /// server instance of the GenHTTP webserver.
    /// </summary>
    /// <param name="configurationHook">An action invoked with the pre-configured web application builder, allowing to customize the underlying ASP.NET app</param>
    /// <param name="applicationHook">An action invoked with the created application instance, allowing to customize the underlying ASP.NET app</param>
    /// <returns>The host which can be used to run a server instance</returns>
    public static IServerHost Create(Action<Builder<WiredHttp11, Http11Context>>? configurationHook = null, Action<WiredApp<Http11Context>>? applicationHook = null)
        => new ServerHost(Server.Create(configurationHook, applicationHook));

}
