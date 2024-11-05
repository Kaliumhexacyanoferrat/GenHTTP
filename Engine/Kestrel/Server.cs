using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Kestrel.Hosting;

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
    /// <returns>The builder to create the instance</returns>
    public static IServerBuilder Create() => new KestrelServerBuilder();

}
