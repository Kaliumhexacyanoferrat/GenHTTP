using System.Net;
using System.Security.Cryptography.X509Certificates;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using Microsoft.AspNetCore.Http;

namespace GenHTTP.Adapters.AspNetCore.Types;

public sealed class ClientConnection : IClientConnection
{

    #region Get-/Setters

    public IPAddress IPAddress => Info.RemoteIpAddress ?? throw new InvalidOperationException("Remote client IP address is not known");

    public ClientProtocol? Protocol { get; }

    public string? Host => Request.Host.HasValue ? Request.Host.Value : null;

    public X509Certificate? Certificate => Info.ClientCertificate;

    private ConnectionInfo Info { get; }

    private HttpRequest Request { get; }

    #endregion

    #region Initialization

    public ClientConnection(ConnectionInfo info, HttpRequest request)
    {
        Info = info;
        Request = request;

        Protocol = (request.IsHttps) ? ClientProtocol.Https : ClientProtocol.Http;
    }

    #endregion

}
