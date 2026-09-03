using System.Net;

namespace GenHTTP.Engine.Ioxide.Infrastructure.Endpoints;

internal sealed class InsecureEndPoint(IPAddress? address, ushort port, bool dualStack, Protocols protocols)
    : EndPoint(address, port, dualStack, protocols)
{
    public override bool Secure => false;
}
