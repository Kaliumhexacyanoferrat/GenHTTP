using System.IO.Pipelines;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Ioxide.Hosting;
using ioxide;

namespace GenHTTP.Engine.Ioxide;

/// <summary>
/// Entry point to host an application using the ioxide io_uring engine.
/// </summary>
public static class Host
{

    /// <param name="configure">
    /// Tunes the ioxide runtime, e.g. <c>c => c with { ReactorCount = 16 }</c>. Listen ports always
    /// come from the GenHTTP endpoint bindings.
    /// </param>
    /// <param name="onReactorStart">
    /// Registers ring-native services on each reactor's own thread before it serves, e.g.
    /// <c>r => PgPool.Start(r, pgOptions)</c>. Handlers resolve them via <c>IoxideReactor.Current</c>.
    /// </param>
    /// <param name="connectionFactory">
    /// Replaces the built-in transport selection. A returned pipe implementing
    /// <see cref="IAsyncDisposable" /> is disposed when the connection ends.
    /// </param>
    /// <param name="options">
    /// Protocols, kernel TLS, the HTTP/3 certificate, mutual TLS and QPACK. Per-endpoint
    /// settings stay on <c>Bind</c>.
    /// </param>
    public static IServerHost Create(Func<ServerConfig, ServerConfig>? configure = null, Action<Reactor>? onReactorStart = null, Func<TcpConnection, ValueTask<IDuplexPipe>>? connectionFactory = null, IoxideOptions? options = null)
        => new IoxideServerHost(configure, onReactorStart, connectionFactory, options);

}
