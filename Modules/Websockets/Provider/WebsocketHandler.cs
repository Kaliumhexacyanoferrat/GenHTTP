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
        var key = ValidateRequest(request);
        
        var content = contentFactory(request);

        var response = request.Respond()
            .Status(ResponseStatus.SwitchingProtocols)
            .Connection(Connection.Upgrade)
            .Header("Upgrade", "websocket")
            .Header("Sec-WebSocket-Accept", Handshake.CreateAcceptKey(key))
            .Content(content)
            .Build();

        return ValueTask.FromResult<IResponse?>(response);
    }

    private static string ValidateRequest(IRequest request)
    {
        if (request.Method.KnownMethod != RequestMethod.Get)
        {
            throw new ProviderException(ResponseStatus.MethodNotAllowed, "Websocket connections can only be initiated via GET");
        }
        
        var key = request.Headers.GetValueOrDefault("Sec-WebSocket-Key")
            ?? throw new ProviderException(ResponseStatus.BadRequest, "Client did not initiate websocket handshake. Header Sec-WebSocket-Key is missing from the request.");

        var version = request.Headers.GetValueOrDefault("Sec-WebSocket-Version");

        if (!int.TryParse(version, out var versionInt) || versionInt != 13)
        {
            throw new ProviderException(ResponseStatus.UpgradeRequired, "Only version 13 is supported by this websocket server.", (b) => b.Header("Sec-WebSocket-Version", "13"));
        }

        return key;
    }

}
