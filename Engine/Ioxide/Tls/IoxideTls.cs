using System.IO.Pipelines;

using ioxide;
using ioxide.tls;

namespace GenHTTP.Engine.Ioxide;

/// <summary>
/// TLS-termination helpers for the ioxide engine. The engine owns the transport plumbing (the
/// decrypt pump + kTLS-TX pipe adapter, <see cref="TlsDuplexPipe"/>); the host supplies the
/// certificate/key and decides which connections to terminate (typically by listener port).
/// </summary>
/// <example>
/// <code>
/// Host.Create(
///     configure: c => c with { ExtraPorts = [8081] },
///     onReactorStart: r => IoxideTls.StartService(r, new TlsOptions { CertificatePath = cert, KeyPath = key }),
///     connectionFactory: conn => conn.ListenerPort == 8081
///         ? IoxideTls.AcceptAsync(conn)
///         : new ValueTask&lt;IDuplexPipe&gt;(new ConnectionDualPipe(conn)));
/// </code>
/// </example>
public static class IoxideTls
{
    /// <summary>
    /// <c>onReactorStart</c> hook: start the ring-native TLS service (OpenSSL context) on this reactor.
    /// </summary>
    public static void StartService(Reactor reactor, TlsOptions options) => TlsService.Start(reactor, options);

    /// <summary>
    /// <c>connectionFactory</c> helper: TLS-terminate <paramref name="conn"/> on the current reactor and
    /// return the duplex pipe the engine serves over. Requires <see cref="StartService"/> to have run.
    /// </summary>
    public static async ValueTask<IDuplexPipe> AcceptAsync(Connection conn)
    {
        var session = await IoxideReactor.Current.GetService<TlsService>().AcceptAsync(conn);
        return new TlsDuplexPipe(conn, session);
    }
}
