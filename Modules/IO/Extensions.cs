using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.IO;

public static class Extensions
{

    /// <summary>
    ///     Configures the server to respond with partial responses if
    ///     requested by the client.
    /// </summary>
    /// <param name="host">The host to add the feature to</param>
    public static IServerHost RangeSupport(this IServerHost host)
    {
        host.Add(IO.RangeSupport.Create());
        return host;
    }
}
