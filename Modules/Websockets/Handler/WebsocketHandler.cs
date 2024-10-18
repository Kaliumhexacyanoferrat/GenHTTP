using Fleck;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using IHandler = GenHTTP.Api.Content.IHandler;

namespace GenHTTP.Modules.Websockets.Handler;

public sealed class WebsocketHandler : IHandler
{

    #region Get-/Setters

    public IHandler Parent { get; }

    public Action<IWebSocketConnection> Handler { get; }

    public List<string> SupportedProtocols { get; }

    #endregion

    #region Initialization

    public WebsocketHandler(IHandler parent, Action<IWebSocketConnection> handler, List<string> supportedProtocols)
    {
        Parent = parent;
        Handler = handler;
        SupportedProtocols = supportedProtocols;
    }

    #endregion

    #region Functionality

    public ValueTask PrepareAsync() => new();

    public ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        if (!request.Headers.ContainsKey("Upgrade") || request.Headers["Upgrade"] != "websocket")
        {
            throw new ProviderException(ResponseStatus.BadRequest, "Websocket upgrade request expected");
        }

        var upgrade = request.Upgrade();

        var connection = new WebsocketConnection(upgrade.Socket, SupportedProtocols, Handler);

        connection.Start(request);

        return new(upgrade.Response);
    }

    #endregion

}
