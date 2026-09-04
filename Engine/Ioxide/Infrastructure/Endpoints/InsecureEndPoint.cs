using System.Net;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Engine.Ioxide.Infrastructure.Endpoints;

/// <summary>An endpoint bound without a certificate, so cleartext only.</summary>
internal sealed class InsecureEndPoint(IPAddress? address, ushort port, bool dualStack, HttpProtocols httpProtocols)
    : EndPoint(address, port, dualStack, httpProtocols)
{
    public override bool Secure => false;
}
