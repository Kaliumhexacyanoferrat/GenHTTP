using System.Net;

namespace GenHTTP.Engine.Ioxide.Infrastructure.Endpoints;

/// <summary>
/// An endpoint bound without a certificate. Cleartext HTTP/1.1 or HTTP/2 only - HTTP/3 is refused
/// on one of these, since QUIC carries TLS 1.3 and has no cleartext mode.
/// </summary>
internal sealed class InsecureEndPoint(IPAddress? address, ushort port, bool dualStack, Protocols protocols)
    : EndPoint(address, port, dualStack, protocols)
{
    public override bool Secure => false;
}
