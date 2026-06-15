using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Ioxide.Hosting;
using GenHTTP.Engine.Shared.Hosting;

using ioxide;

namespace GenHTTP.Engine.Ioxide;

/// <summary>
/// Entry point to host an application using the ioxide io_uring engine.
/// </summary>
public static class Host
{
    /// <param name="configure">
    /// Optional hook to tune the ioxide runtime (reactor count, ring sizes, recv/write
    /// buffers, ...). Receives a config pre-seeded with sensible defaults; return a
    /// modified copy, e.g. <c>c => c with { ReactorCount = 16 }</c>. The listen port is
    /// always taken from the GenHTTP endpoint binding (<c>.Port()</c>/<c>.Bind()</c>).
    /// </param>
    public static IServerHost Create(Func<ServerConfig, ServerConfig>? configure = null) => new ServerHost(Server.Create(configure));
}

/// <summary>
/// Creates server builders backed by the ioxide io_uring runtime.
/// </summary>
public static class Server
{
    public static IServerBuilder Create(Func<ServerConfig, ServerConfig>? configure = null) => new IoxideServerBuilder(configure);
}
