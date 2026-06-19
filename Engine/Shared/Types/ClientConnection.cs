using System.Net;
using System.Security.Cryptography.X509Certificates;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Shared.Types;

public sealed class ClientConnection : IClientConnection
{

    public IPAddress? IPAddress { get; private set; }

    public ClientProtocol? Protocol { get; private set; }

    public X509Certificate? Certificate { get; private set; }

    public void Apply(IPAddress? address, ClientProtocol? protocol, X509Certificate? certificate)
    {
        IPAddress = address;
        Protocol = protocol;
        Certificate = certificate;
    }

    public void Reset()
    {
        IPAddress = null;
        Protocol = null;
        Certificate = null;
    }

}
