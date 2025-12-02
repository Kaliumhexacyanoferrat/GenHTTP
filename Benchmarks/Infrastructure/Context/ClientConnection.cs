using System.Net;
using System.Security.Cryptography.X509Certificates;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Benchmarks.Infrastructure.Context;

public class ClientConnection : IClientConnection
{

    #region Get-/Setters

    public IPAddress? IPAddress { get; set; }

    public ClientProtocol? Protocol { get; set; }

    public string? Host { get; set; }

    public X509Certificate? Certificate { get; set; }

    #endregion

    #region Initialization

    public ClientConnection()
    {
        Protocol = ClientProtocol.Http;
    }

    #endregion

}
