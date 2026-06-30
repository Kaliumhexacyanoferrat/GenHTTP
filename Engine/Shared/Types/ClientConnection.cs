using System.Net;
using System.Security.Cryptography.X509Certificates;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Shared.Types;

public sealed class ClientConnection : IClientConnection
{

    public IPAddress? Address { get; private set; }

    public ClientProtocol? Protocol { get; private set; }

    public X509Certificate? Certificate { get; private set; }

    public void Apply(IPAddress? address, ClientProtocol? protocol, X509Certificate? certificate)
    {
        Address = address;
        Protocol = protocol;
        Certificate = certificate;
    }

    public void Reset()
    {
        Address = null;
        Protocol = null;
        Certificate = null;
    }

}
