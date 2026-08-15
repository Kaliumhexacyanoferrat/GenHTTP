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
    /// Optional hook to turn an accepted <see cref="TcpConnection" /> into the duplex pipe the engine
    /// serves it over, overriding the built-in transport selection (plain pipe, or TLS termination
    /// for endpoints bound with a certificate). A returned pipe implementing
    /// <see cref="IAsyncDisposable" /> is disposed when the connection ends.
    /// </param>
    /// <param name="kernelTx">
    /// Offload TLS record ENCRYPTION to the kernel (kTLS TX) on TLS-terminated endpoints instead of
    /// encrypting in OpenSSL. Off by default (OpenSSL both ways). The kernel produces the records on
    /// the send path while OpenSSL still drives the handshake; requires the Linux <c>tls</c> module
    /// and TLS 1.3.
    /// </param>
    /// <param name="kernelRx">
    /// Offload TLS record DECRYPTION to the kernel (kTLS RX) on the receive path. Off by default and
    /// experimental; it requires <paramref name="kernelTx"/> (RX shares the ULP handoff TX installs,
    /// so ioxide refuses RX alone) and a peer that sends no post-handshake control records.
    /// </param>
    /// <param name="options">
    /// Protocol and TLS options for the engine: HTTP/2, the HTTP/3 certificate, mutual TLS and
    /// QPACK. Endpoint-level settings stay on <c>Bind</c> - the port, its certificate, whether it
    /// serves HTTP/3 (<c>enableQuic</c>) and whether it asks for a client certificate.
    /// </param>
    public static IServerHost Create(Func<ServerConfig, ServerConfig>? configure = null, Action<Reactor>? onReactorStart = null, Func<TcpConnection, ValueTask<IDuplexPipe>>? connectionFactory = null, bool kernelTx = false, bool kernelRx = false, IoxideOptions? options = null)
        => new IoxideServerHost(configure, onReactorStart, connectionFactory, kernelTx, kernelRx, options);
    
}
