using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Websockets.Utils;
using IHandler = GenHTTP.Api.Content.IHandler;

namespace GenHTTP.Modules.Websockets.Legacy;

public sealed class WebsocketHandler : IHandler
{

    #region Get-/Setters

    public Func<IWebsocketConnection, Task>? OnOpen { get; }

    public Func<IWebsocketConnection, Task>? OnClose { get; }

    public Func<IWebsocketConnection, string, Task>? OnMessage { get; }

    public Func<IWebsocketConnection, byte[], Task>? OnBinary { get; }

    public Func<IWebsocketConnection, byte[], Task>? OnPing { get; }

    public Func<IWebsocketConnection, byte[], Task>? OnPong { get; }

    public Func<IWebsocketConnection, Exception, Task>? OnError { get; }

    public List<string> SupportedProtocols { get; }

    #endregion

    #region Initialization

    public WebsocketHandler(List<string> supportedProtocols,
        Func<IWebsocketConnection, Task>? onOpen,
        Func<IWebsocketConnection, Task>? onClose,
        Func<IWebsocketConnection, string, Task>? onMessage,
        Func<IWebsocketConnection, byte[], Task>? onBinary,
        Func<IWebsocketConnection, byte[], Task>? onPing,
        Func<IWebsocketConnection, byte[], Task>? onPong,
        Func<IWebsocketConnection, Exception, Task>? onError)
    {
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

        var socket = new WrappedSocket(upgrade);

        // the server will re-use the request for a new client, so
        // we need to freeze the values to allow access from the socket context
        var clone = ClonedRequest.From(request);

        var connection = new WebsocketConnection(socket, clone, SupportedProtocols, OnOpen, OnClose, OnMessage, OnBinary, OnPing, OnPong, OnError);

        connection.Start();

        return new(upgrade.Response);
    }

    #endregion

}
