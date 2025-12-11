using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Websockets.Protocol;
using GenHTTP.Modules.Websockets.Provider;

namespace GenHTTP.Modules.ReverseProxy.Websocket;

public sealed class WebsocketProxy : IHandler
{
    private readonly string _upstream;
    
    public WebsocketProxy(string upstream)
    {
        _upstream = upstream;
    }
    
    public ValueTask PrepareAsync() => ValueTask.CompletedTask;

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var upstreamConnection = new RawWebsocketConnection(_upstream);
        
        await upstreamConnection.InitializeStream();
        
        using var upgradeCts = new CancellationTokenSource(5000);
        
        try
        {
            if (!await upstreamConnection.TryUpgrade(request, token: upgradeCts.Token))
            {
                return request
                    .Respond()
                    .Status(ResponseStatus.BadGateway)
                    .Build();
            }
        }
        catch (Exception e)
        {
            throw new InvalidOperationException("Failed to upgrade upstream.", e);
        }

        var websocketHandler = new WebsocketHandler(_ => new WebsocketTunnelContent(upstreamConnection));
        
        return await websocketHandler.HandleAsync(request);
    }
    
}