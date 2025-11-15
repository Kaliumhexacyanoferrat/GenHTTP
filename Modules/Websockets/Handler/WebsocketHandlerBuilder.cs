using GenHTTP.Api.Content;

namespace GenHTTP.Modules.Websockets.Handler;

public class WebsocketHandlerBuilder : IHandlerBuilder<WebsocketHandlerBuilder>
{
    private readonly List<IConcernBuilder> _concerns = [];

    private readonly List<string> _supportedProtocols = [];

    private Func<IWebsocketConnection, Task>? _onOpen;
    private Func<IWebsocketConnection, Task>? _onClose;
    private Func<IWebsocketConnection, string, Task>? _onMessage;
    private Func<IWebsocketConnection, byte[], Task>? _onBinary;
    private Func<IWebsocketConnection, byte[], Task>? _onPing;
    private Func<IWebsocketConnection, byte[], Task>? _onPong;
    private Func<IWebsocketConnection, Exception, Task>? _onError;

    #region Functionality

    public WebsocketHandlerBuilder Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);
        return this;
    }

    /// <summary>
    /// Specifies a protocol supported by this websocket server.
    /// </summary>
    /// <param name="supportedProtocol">The name of the supported protocol</param>
    public WebsocketHandlerBuilder Protocol(string supportedProtocol)
    {
        _supportedProtocols.Add(supportedProtocol);
        return this;
    }

    /// <summary>
    /// Will be executed if a new websocket client connected.
    /// </summary>
    /// <param name="handler">The method to be executed</param>
    public WebsocketHandlerBuilder OnOpen(Func<IWebsocketConnection, Task> handler)
    {
        _onOpen = handler;
        return this;
    }

    /// <summary>
    /// Will be executed if a websocket client disconnects.
    /// </summary>
    /// <param name="handler">The method to be executed</param>
    public WebsocketHandlerBuilder OnClose(Func<IWebsocketConnection, Task> handler)
    {
        _onClose = handler;
        return this;
    }

    /// <summary>
    /// Will be executed if a string message has been received from the client.
    /// </summary>
    /// <param name="handler">The method to be executed</param>
    public WebsocketHandlerBuilder OnMessage(Func<IWebsocketConnection, string, Task> handler)
    {
        _onMessage = handler;
        return this;
    }

    /// <summary>
    /// Will be executed if a binary message has been received from the client.
    /// </summary>
    /// <param name="handler">The method to be executed</param>
    public WebsocketHandlerBuilder OnBinary(Func<IWebsocketConnection, byte[], Task> handler)
    {
        _onBinary = handler;
        return this;
    }

    /// <summary>
    /// Will be executed if the client sends a ping request.
    /// </summary>
    /// <param name="handler">The method to be executed</param>
    public WebsocketHandlerBuilder OnPing(Func<IWebsocketConnection, byte[], Task> handler)
    {
        _onPing = handler;
        return this;
    }

    /// <summary>
    /// Will be executed if the client sends a pong request.
    /// </summary>
    /// <param name="handler">The method to be executed</param>
    public WebsocketHandlerBuilder OnPong(Func<IWebsocketConnection, byte[], Task> handler)
    {
        _onPong = handler;
        return this;
    }

    /// <summary>
    /// Will be executed if there is some client connection error.
    /// </summary>
    /// <param name="handler">The method to be executed</param>
    public WebsocketHandlerBuilder OnError(Func<IWebsocketConnection, Exception, Task> handler)
    {
        _onError = handler;
        return this;
    }

    public IHandler Build()
    {
        return Concerns.Chain(_concerns, new WebsocketHandler(_supportedProtocols, _onOpen, _onClose, _onMessage, _onBinary, _onPing, _onPong, _onError));
    }

    #endregion

}
