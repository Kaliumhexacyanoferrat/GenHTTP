using GenHTTP.Api.Infrastructure;
using GenHTTP.Engine.Hosting;

namespace GenHTTP.Engine;

public static class Host
{

    /// <summary>
    /// Provides a new server host that can be used to run a
    /// server instance of the GenHTTP webserver.
    /// </summary>
    /// <returns>The host which can be used to run a server instance</returns>
    public static IServerHost Create() => new ServerHost();

}
