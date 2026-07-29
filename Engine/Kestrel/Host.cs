using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Kestrel.Hosting;

namespace GenHTTP.Engine.Kestrel;

public static class Host
{

    /// <summary>
    /// Provides a new server host that runs the GenHTTP webserver on Kestrel.
    /// </summary>
    public static IServerHost Create() => new KestrelServerHost();

}
