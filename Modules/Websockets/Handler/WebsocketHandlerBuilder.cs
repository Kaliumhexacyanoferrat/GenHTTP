using GenHTTP.Api.Content;

namespace GenHTTP.Modules.Websockets.Handler;

public class WebsocketHandlerBuilder : IHandlerBuilder<WebsocketHandlerBuilder>
{
    private readonly List<IConcernBuilder> _Concerns = [];

    private readonly List<string> _SupportedProtocols = [];

    private Action<IWebsocketConnection>? _OnOpen;
    private Action<IWebsocketConnection>? _OnClose;
    private Action<IWebsocketConnection, string>? _OnMessage;
    private Action<IWebsocketConnection, byte[]>? _OnBinary;
    private Action<IWebsocketConnection, byte[]>? _OnPing;
    private Action<IWebsocketConnection, byte[]>? _OnPong;
    private Action<IWebsocketConnection, Exception>? _OnError;

    #region Functionality

    public WebsocketHandlerBuilder Add(IConcernBuilder concern)
    {
        _Concerns.Add(concern);
        return this;
    }

    /// <summary>
    /// Specifies a protocol supported by this websocket server.
    /// </summary>
    /// <param name="supportedProtocol">The name of the supported protocol</param>
    public WebsocketHandlerBuilder Protocol(string supportedProtocol)
    {
        _SupportedProtocols.Add(supportedProtocol);
        return this;
    }

    /// <summary>
    /// Will be executed if a new websocket client connected.
    /// </summary>
    /// <param name="handler">The method to be executed</param>
    public WebsocketHandlerBuilder OnOpen(Action<IWebsocketConnection> handler)
    {
        _OnOpen = handler;
        return this;
    }

    /// <summary>
    /// Will be executed if a websocket client disconnects.
    /// </summary>
    /// <param name="handler">The method to be executed</param>
    public WebsocketHandlerBuilder OnClose(Action<IWebsocketConnection> handler)
    {
        _OnClose = handler;
        return this;
    }

    /// <summary>
    /// Will be executed if a string message has been received from the client.
    /// </summary>
    /// <param name="handler">The method to be executed</param>
    public WebsocketHandlerBuilder OnMessage(Action<IWebsocketConnection, string> handler)
    {
        _OnMessage = handler;
        return this;
    }

    /// <summary>
    /// Will be executed if a binary message has been received from the client.
    /// </summary>
    /// <param name="handler">The method to be executed</param>
    public WebsocketHandlerBuilder OnBinary(Action<IWebsocketConnection, byte[]> handler)
    {
        _OnBinary = handler;
        return this;
    }

    /// <summary>
    /// Will be executed if the client sends a ping request.
    /// </summary>
    /// <param name="handler">The method to be executed</param>
    public WebsocketHandlerBuilder OnPing(Action<IWebsocketConnection, byte[]> handler)
    {
        _OnPing = handler;
        return this;
    }

    /// <summary>
    /// Will be executed if the client sends a pong request.
    /// </summary>
    /// <param name="handler">The method to be executed</param>
    public WebsocketHandlerBuilder OnPong(Action<IWebsocketConnection, byte[]> handler)
    {
        _OnPong = handler;
        return this;
    }

    /// <summary>
    /// Will be executed if there is some client connection error.
    /// </summary>
    /// <param name="handler">The method to be executed</param>
    public WebsocketHandlerBuilder OnError(Action<IWebsocketConnection, Exception> handler)
    {
        _OnError = handler;
        return this;
    }

    public IHandler Build()
    {
        return Concerns.Chain(_Concerns, new WebsocketHandler(_SupportedProtocols, _OnOpen, _OnClose, _OnMessage, _OnBinary, _OnPing, _OnPong, _OnError));
    }

    #endregion

}
