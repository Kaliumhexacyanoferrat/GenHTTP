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
    /// <param name="kernelTx">
    /// Encrypt TLS records in the kernel (kTLS TX) rather than OpenSSL, which still handshakes.
    /// Needs the Linux <c>tls</c> module and TLS 1.3.
    /// </param>
    /// <param name="kernelRx">
    /// Decrypt in the kernel too. Experimental: needs <paramref name="kernelTx"/> (RX shares the
    /// ULP handoff TX installs) and a peer sending no post-handshake control records.
    /// </param>
    /// <param name="options">
    /// Protocols, the HTTP/3 certificate, mutual TLS and QPACK. Per-endpoint settings stay on
    /// <c>Bind</c>.
    /// </param>
    public static IServerHost Create(Func<ServerConfig, ServerConfig>? configure = null, Action<Reactor>? onReactorStart = null, Func<TcpConnection, ValueTask<IDuplexPipe>>? connectionFactory = null, bool kernelTx = false, bool kernelRx = false, IoxideOptions? options = null)
        => new IoxideServerHost(configure, onReactorStart, connectionFactory, kernelTx, kernelRx, options);

}
