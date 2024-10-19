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

    public WebsocketHandlerBuilder Protocol(string supportedProtocol)
    {
        _SupportedProtocols.Add(supportedProtocol);
        return this;
    }

    public WebsocketHandlerBuilder OnOpen(Action<IWebsocketConnection> handler)
    {
        _OnOpen = handler;
        return this;
    }

    public WebsocketHandlerBuilder OnClose(Action<IWebsocketConnection> handler)
    {
        _OnClose = handler;
        return this;
    }

    public WebsocketHandlerBuilder OnMessage(Action<IWebsocketConnection, string> handler)
    {
        _OnMessage = handler;
        return this;
    }

    public WebsocketHandlerBuilder OnBinary(Action<IWebsocketConnection, byte[]> handler)
    {
        _OnBinary = handler;
        return this;
    }

    public WebsocketHandlerBuilder OnPing(Action<IWebsocketConnection, byte[]> handler)
    {
        _OnPing = handler;
        return this;
    }

    public WebsocketHandlerBuilder OnPong(Action<IWebsocketConnection, byte[]> handler)
    {
        _OnPong = handler;
        return this;
    }

    public WebsocketHandlerBuilder OnError(Action<IWebsocketConnection, Exception> handler)
    {
        _OnError = handler;
        return this;
    }

    public IHandler Build(IHandler parent)
    {
        return Concerns.Chain(parent, _Concerns, (p) => new WebsocketHandler(p, _SupportedProtocols, _OnOpen, _OnClose, _OnMessage, _OnBinary, _OnPing, _OnPong, _OnError));
    }

    #endregion

}
