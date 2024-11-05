using System.Net;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using Microsoft.AspNetCore.Http;

namespace GenHTTP.Engine.Kestrel.Hosting;

public class ClientConnection : IClientConnection
{

    #region Get-/Setters

    public IPAddress IPAddress => Info.RemoteIpAddress ?? throw new InvalidOperationException("Remote client IP address is not known");

    public ClientProtocol? Protocol => ClientProtocol.Http; // todo

    public string? Host => Request.Host.HasValue ? Request.Host.Value : null;

    private ConnectionInfo Info { get; }

    private HttpRequest Request { get; }

    #endregion

    #region Initialization

    public ClientConnection(ConnectionInfo info, HttpRequest request)
    {
        Info = info;
        Request = request;
    }

    #endregion

}
