using System.Net;
using System.Security.Cryptography.X509Certificates;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using Wired.IO.Http11Express.Request;

namespace GenHTTP.Adapters.WiredIO.Types;

public sealed class ClientConnection : IClientConnection
{

    #region Get-/Setters

    public IPAddress IPAddress => throw new InvalidOperationException("Remote client IP address is not known");

    public ClientProtocol? Protocol { get; }

    public string? Host => Request.Headers.GetValueOrDefault("Host");

    public X509Certificate? Certificate => null;

    private IExpressRequest Request { get; }

    #endregion

    #region Initialization

    public ClientConnection(IExpressRequest request)
    {
        Request = request;

        // todo: wired does not expose this information
        Protocol = ClientProtocol.Http;
    }

    #endregion

}
