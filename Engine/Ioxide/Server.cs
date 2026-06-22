using System.IO.Pipelines;

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
    /// <param name="onReactorStart">
    /// Optional hook invoked once per reactor, on that reactor's own thread, before it serves.
    /// Use it to register per-reactor (ring-native) services on the supplied <see cref="Reactor" />
    /// — e.g. <c>r => PgPool.Start(r, pgOptions)</c> — which handler code can later resolve via
    /// <see cref="Hosting.IoxideServer" />'s reactor seam (<c>IoxideReactor.Current</c>).
    /// </param>
    /// <param name="connectionFactory">
    /// Optional hook to turn an accepted <see cref="Connection" /> into the duplex pipe the engine
    /// serves it over. Defaults to a plain <c>ConnectionDualPipe</c>. Supply a custom factory to
    /// wrap the transport — e.g. terminate TLS on a second listener port by decrypting inbound bytes
    /// and writing plaintext for kTLS TX. A returned pipe implementing <see cref="IAsyncDisposable" />
    /// is disposed when the connection ends.
    /// </param>
    public static IServerHost Create(Func<ServerConfig, ServerConfig>? configure = null, Action<Reactor>? onReactorStart = null, Func<Connection, ValueTask<IDuplexPipe>>? connectionFactory = null) 
        => new IoxideServerHost(configure, onReactorStart, connectionFactory);
    
}
