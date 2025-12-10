using System.Security.Cryptography;
using System.Text;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Websockets.Handler;

namespace GenHTTP.Modules.ReverseProxy.Websocket;

public class WebsocketProxy : IHandler
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
        
        var upgradeCts = new CancellationTokenSource(5000);
        
        try
        {
            if (!await upstreamConnection.TryUpgrade(request.Headers, token: upgradeCts.Token))
            {
                throw new InvalidOperationException("Failed to upgrade upstream.");
            }
        }
        catch (Exception e)
        {
            throw new InvalidOperationException("Failed to upgrade upstream.", e);
        }

        var key = request.Headers.GetValueOrDefault("Sec-WebSocket-Key")
                  ?? throw new InvalidOperationException("Sec-WebSocket-Key not found");
        
        var response = request.Respond()
            .Status(ResponseStatus.SwitchingProtocols)
            .Connection(Connection.Upgrade)
            .Header("Upgrade", "websocket")
            .Header("Sec-WebSocket-Accept", Handshake.(key))
            .Content(new WebsocketTunnelContent(upstreamConnection))
            .Build();
        
        return response;
    }
    
}