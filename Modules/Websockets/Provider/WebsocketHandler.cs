using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Websockets.Protocol;

namespace GenHTTP.Modules.Websockets.Provider;

public class WebsocketHandler(IResponseContent content) : IHandler
{
    
    public ValueTask PrepareAsync() => ValueTask.CompletedTask;

    public ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var key = request.Headers.GetValueOrDefault("Sec-WebSocket-Key")
                  ?? throw new InvalidOperationException("Sec-WebSocket-Key not found");

        var response = request.Respond()
            .Status(ResponseStatus.SwitchingProtocols)
            .Connection(Connection.Upgrade)
            .Header("Upgrade", "websocket")
            .Header("Sec-WebSocket-Accept", Handshake.CreateAcceptKey(key))
            .Content(content)
            .Build();

        return ValueTask.FromResult<IResponse?>(response);
    }
    
}