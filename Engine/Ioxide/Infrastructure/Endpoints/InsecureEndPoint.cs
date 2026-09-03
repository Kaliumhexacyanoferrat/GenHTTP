using System.Net;

namespace GenHTTP.Engine.Ioxide.Infrastructure.Endpoints;

/// <summary>An endpoint bound without a certificate, so cleartext only.</summary>
internal sealed class InsecureEndPoint(IPAddress? address, ushort port, bool dualStack, Protocols protocols)
    : EndPoint(address, port, dualStack, protocols)
{
    public override bool Secure => false;
}
