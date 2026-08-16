using System.IO.Pipelines;

using ioxide;
using ioxide.tls;

namespace GenHTTP.Engine.Ioxide;

/// <summary>
/// TLS termination for the endpoints bound with a certificate.
/// </summary>
internal static class IoxideTls
{
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

/// <summary>
/// The TLS service each secure port owns on this reactor. One per port, since ALPN and the client
/// CA differ per endpoint; resolved by the listener port a connection arrived on.
/// </summary>
internal sealed class TlsRegistry
{
    private readonly Dictionary<ushort, TlsService> _byPort = [];

    public void Add(ushort port, TlsService service) => _byPort[port] = service;

    public bool TryFor(ushort port, out TlsService service) => _byPort.TryGetValue(port, out service!);
}
