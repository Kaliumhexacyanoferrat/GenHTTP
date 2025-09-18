using GenHTTP.Api.Content;

namespace GenHTTP.Modules.Websockets.Handler;

public class WebsocketHandlerBuilder : IHandlerBuilder<WebsocketHandlerBuilder>
{
    private readonly List<IConcernBuilder> _Concerns = [];

    private readonly List<string> _SupportedProtocols = [];

    private Func<IWebsocketConnection, Task>? _OnOpen;
    private Func<IWebsocketConnection, Task>? _OnClose;
    private Func<IWebsocketConnection, string, Task>? _OnMessage;
    private Func<IWebsocketConnection, byte[], Task>? _OnBinary;
    private Func<IWebsocketConnection, byte[], Task>? _OnPing;
    private Func<IWebsocketConnection, byte[], Task>? _OnPong;
    private Func<IWebsocketConnection, Exception, Task>? _OnError;

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
    public WebsocketHandlerBuilder OnOpen(Func<IWebsocketConnection, Task> handler)
    {
        _OnOpen = handler;
        return this;
    }

    /// <summary>
    /// Will be executed if a websocket client disconnects.
    /// </summary>
    /// <param name="handler">The method to be executed</param>
    public WebsocketHandlerBuilder OnClose(Func<IWebsocketConnection, Task> handler)
    {
        _OnClose = handler;
        return this;
    }

    /// <summary>
    /// Will be executed if a string message has been received from the client.
    /// </summary>
    /// <param name="handler">The method to be executed</param>
    public WebsocketHandlerBuilder OnMessage(Func<IWebsocketConnection, string, Task> handler)
    {
        _OnMessage = handler;
        return this;
    }

    /// <summary>
    /// Will be executed if a binary message has been received from the client.
    /// </summary>
    /// <param name="handler">The method to be executed</param>
    public WebsocketHandlerBuilder OnBinary(Func<IWebsocketConnection, byte[], Task> handler)
    {
        _OnBinary = handler;
        return this;
    }

    /// <summary>
    /// Will be executed if the client sends a ping request.
    /// </summary>
    /// <param name="handler">The method to be executed</param>
    public WebsocketHandlerBuilder OnPing(Func<IWebsocketConnection, byte[], Task> handler)
    {
        _OnPing = handler;
        return this;
    }

    /// <summary>
    /// Will be executed if the client sends a pong request.
    /// </summary>
    /// <param name="handler">The method to be executed</param>
    public WebsocketHandlerBuilder OnPong(Func<IWebsocketConnection, byte[], Task> handler)
    {
        _OnPong = handler;
        return this;
    }

    /// <summary>
    /// Will be executed if there is some client connection error.
    /// </summary>
    /// <param name="handler">The method to be executed</param>
    public WebsocketHandlerBuilder OnError(Func<IWebsocketConnection, Exception, Task> handler)
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
