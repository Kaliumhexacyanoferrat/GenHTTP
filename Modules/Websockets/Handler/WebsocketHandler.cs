using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using IHandler = GenHTTP.Api.Content.IHandler;

namespace GenHTTP.Modules.Websockets.Handler;

public sealed class WebsocketHandler : IHandler
{

    #region Get-/Setters

    public IHandler Parent { get; }

    public Action<IWebsocketConnection>? OnOpen { get; }

    public Action<IWebsocketConnection>? OnClose { get; }

    public Action<IWebsocketConnection, string>? OnMessage { get; }

    public Action<IWebsocketConnection, byte[]>? OnBinary { get; }

    public Action<IWebsocketConnection, byte[]>? OnPing { get; }

    public Action<IWebsocketConnection, byte[]>? OnPong { get; }

    public Action<IWebsocketConnection, Exception>? OnError { get; }

    public List<string> SupportedProtocols { get; }

    #endregion

    #region Initialization

    public WebsocketHandler(IHandler parent, List<string> supportedProtocols,
        Action<IWebsocketConnection>? onOpen,
        Action<IWebsocketConnection>? onClose,
        Action<IWebsocketConnection, string>? onMessage,
        Action<IWebsocketConnection, byte[]>? onBinary,
        Action<IWebsocketConnection, byte[]>? onPing,
        Action<IWebsocketConnection, byte[]>? onPong,
        Action<IWebsocketConnection, Exception>? onError)
    {
        Parent = parent;   
        SupportedProtocols = supportedProtocols;

        OnOpen = onOpen;
        OnClose = onClose;
        OnMessage = onMessage;
        OnBinary = onBinary;
        OnPing = onPing;
        OnPong = onPong;
        OnError = onError;
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

        var connection = new WebsocketConnection(upgrade.Socket, request, SupportedProtocols, OnOpen, OnClose, OnMessage, OnBinary, OnPing, OnPong, OnError);

        connection.Start();

        return new(upgrade.Response);
    }

    #endregion

}
