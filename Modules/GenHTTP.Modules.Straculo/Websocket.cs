using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Straculo.Imperative;
using GenHTTP.Modules.Straculo.Protocol;
using GenHTTP.Modules.Straculo.Provider;
using GenHTTP.Modules.Straculo.Reactive;

namespace GenHTTP.Modules.Straculo;

public static class Websocket
{
    public static WebsocketBuilder Create() => new();
    
    public static ReactiveWebsocketBuilder CreateReactive(int rxBufferSize = 8192) => new(rxBufferSize);
    
    public static IResponse CreateWebsocketResponse(IRequest request, WebsocketContent content)
    {
        var key = request.Headers.GetValueOrDefault("Sec-WebSocket-Key");

        if (key is null)
        {
            throw new InvalidOperationException("Sec-WebSocket-Key not found");
        }

        return request.Respond()
            .Status(ResponseStatus.SwitchingProtocols)
            .Connection(Connection.Upgrade)
            .Header("Upgrade", "websocket")
            .Header("Sec-WebSocket-Accept", Handshake.CreateAcceptKey(key))
            .Content(content)
            .Build();
    }
}