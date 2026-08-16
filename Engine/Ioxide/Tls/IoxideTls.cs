using System.IO.Pipelines;

using ioxide;
using ioxide.tls;

namespace GenHTTP.Engine.Ioxide;

/// <summary>
/// TLS helpers for hosts wiring a custom <c>connectionFactory</c>. Endpoints bound with a
/// certificate are terminated automatically and need none of this.
/// </summary>
public static class IoxideTls
{
    /// <summary>
    /// <c>onReactorStart</c> hook: start a ring-native TLS service (OpenSSL context) on this reactor.
    /// </summary>
    public static void StartService(Reactor reactor, TlsOptions options) => TlsService.Start(reactor, options);

    /// <summary>
    /// <c>connectionFactory</c> helper: TLS-terminate <paramref name="conn"/> on the current reactor and
    /// return the duplex pipe the engine serves over. Requires <see cref="StartService"/> to have run.
    /// </summary>
    public static async ValueTask<IDuplexPipe> AcceptAsync(TcpConnection conn)
        => await AcceptAsync(conn, IoxideReactor.Current.GetService<TlsService>());

    internal static async ValueTask<IDuplexPipe> AcceptAsync(TcpConnection conn, TlsService service)
        => (await AcceptWithAlpnAsync(conn, service)).Pipe;

    /// <summary>
    /// Terminates TLS and reports what ALPN settled on, which is how a port serving several
    /// protocols knows which one this connection speaks. Null means the client offered nothing the
    /// port lists, and HTTP/1.1 is assumed.
    /// </summary>
    internal static async ValueTask<(IDuplexPipe Pipe, string? Protocol)> AcceptWithAlpnAsync(TcpConnection conn, TlsService service)
    {
        var session = await service.AcceptAsync(conn);

        return (new TlsConnectionDualPipe(conn, session), session.NegotiatedAlpn);
    }
}

internal sealed class TlsRegistry
{
    private readonly Dictionary<ushort, TlsService> _byPort = [];

    public void Add(ushort port, TlsService service) => _byPort[port] = service;

    public bool TryFor(ushort port, out TlsService service) => _byPort.TryGetValue(port, out service!);
}
