using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Web;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.ReverseProxy.Http;
using GenHTTP.Modules.ReverseProxy.Websocket;

namespace GenHTTP.Modules.ReverseProxy.Provider;

public sealed class ReverseProxyProvider : IHandler
{
    
    #region Get-/Setters

    private HttpProxy HttpProxy { get; }

    private WebsocketProxy WebsocketProxy { get; }
    
    #endregion

    #region Initialization

    public ReverseProxyProvider(string upstream, HttpClient client)
    {
        HttpProxy = new HttpProxy(upstream, client);
        WebsocketProxy = new WebsocketProxy(upstream);
    }

    #endregion

    #region Functionality

    public ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        try
        {
            if (request.Headers.ContainsKey("Upgrade") && request.Headers["Upgrade"] == "websocket")
            {
                return WebsocketProxy.HandleAsync(request);
            }
            
            return HttpProxy.HandleAsync(request);
        }
        catch (OperationCanceledException e)
        {
            throw new ProviderException(ResponseStatus.GatewayTimeout, "The gateway did not respond in time.", e);
        }
        catch (HttpRequestException e)
        {
            throw new ProviderException(ResponseStatus.BadGateway, "Unable to retrieve a response from the gateway.", e);
        }
    }

    public ValueTask PrepareAsync() => ValueTask.CompletedTask;

    #endregion

}
