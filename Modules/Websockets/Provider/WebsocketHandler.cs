using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Websockets.Protocol;
using GenHTTP.Modules.Websockets.Utils;

namespace GenHTTP.Modules.Websockets.Provider;

public class WebsocketHandler(Func<IRequest, IResponseContent> contentFactory) : IHandler
{

    public ValueTask PrepareAsync() => ValueTask.CompletedTask;

    public ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        Console.WriteLine("0: Enter IHandler");

        var key = request.Headers.GetValueOrDefault("Sec-WebSocket-Key")
                  ?? throw new InvalidOperationException("Sec-WebSocket-Key not found");

        Console.WriteLine("0: Got Key");

        var frozenRequest = ClonedRequest.From(request);

        var content = contentFactory(frozenRequest);

        Console.WriteLine("0: Obtained Content");

        var response = request.Respond()
            .Status(ResponseStatus.SwitchingProtocols)
            .Connection(Connection.Upgrade)
            .Header("Upgrade", "websocket")
            .Header("Sec-WebSocket-Accept", Handshake.CreateAcceptKey(key))
            .Content(content)
            .Build();

        Console.WriteLine("0: Built Response");

        return ValueTask.FromResult<IResponse?>(response);
    }

}
