using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Websockets.Protocol;

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
                throw new InvalidOperationException("Failed to upgrade upstream.");
            }
        }
        catch (Exception e)
        {
            throw new InvalidOperationException("Failed to upgrade upstream.", e);
        }

        var key = request.Headers.GetValueOrDefault("Sec-WebSocket-Key")
                  ?? throw new ProviderException(ResponseStatus.BadRequest, "Sec-WebSocket-Key not found");
        
        var response = request.Respond()
            .Status(ResponseStatus.SwitchingProtocols)
            .Connection(Connection.Upgrade)
            .Header("Upgrade", "websocket")
            .Header("Sec-WebSocket-Accept", Handshake.CreateAcceptKey(key))
            .Content(new WebsocketTunnelContent(upstreamConnection))
            .Build();
        
        return response;
    }
    
}