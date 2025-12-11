using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

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
        if (request.Headers.TryGetValue("Upgrade", out var upgradeHeader) && upgradeHeader == "websocket")
        {
            return WebsocketProxy.HandleAsync(request);
        }

        return HttpProxy.HandleAsync(request);
    }

    public ValueTask PrepareAsync() => ValueTask.CompletedTask;

    #endregion

}
