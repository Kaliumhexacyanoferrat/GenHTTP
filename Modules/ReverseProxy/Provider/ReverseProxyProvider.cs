using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Protocol.Raw;

using GenHTTP.Modules.ReverseProxy.Http;
using GenHTTP.Modules.ReverseProxy.Websocket;

namespace GenHTTP.Modules.ReverseProxy.Provider;

public sealed class ReverseProxyProvider : IHandler
{
    private static readonly ReadOnlyMemory<byte> UpgradeHeader = "Upgrade"u8.ToArray();

    private static readonly ReadOnlyMemory<byte> WebsocketValue = "websocket"u8.ToArray();

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
        var upgradeHeader = request.Raw.Header.Headers.GetEntry(UpgradeHeader);

        if (upgradeHeader != null && upgradeHeader.Value.Span.SequenceEqual(WebsocketValue.Span))
        {
            return WebsocketProxy.HandleAsync(request);
        }

        return HttpProxy.HandleAsync(request);
    }

    public ValueTask PrepareAsync() => ValueTask.CompletedTask;

    #endregion

}
