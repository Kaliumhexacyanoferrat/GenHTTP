using Fleck;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

using IHandler = GenHTTP.Api.Content.IHandler;

namespace GenHTTP.Modules.Websockets.Handler;

public class WebsocketHandlerBuilder : IHandlerBuilder<WebsocketHandlerBuilder>
{
    private readonly List<IConcernBuilder> _Concerns = [];

    private Action<IWebSocketConnection>? _Handler;

    private readonly List<string> _SupportedProtocols = [];

    #region Functionality

    public WebsocketHandlerBuilder Add(IConcernBuilder concern)
    {
        _Concerns.Add(concern);
        return this;
    }

    public WebsocketHandlerBuilder Handler(Action<IWebSocketConnection> handler)
    {
        _Handler = handler;
        return this;
    }

    public WebsocketHandlerBuilder Protocol(string supportedProtocol)
    {
        _SupportedProtocols.Add(supportedProtocol);
        return this;
    }

    public IHandler Build(IHandler parent)
    {
        var handler = _Handler ?? throw new BuilderMissingPropertyException("Handler");

        return Concerns.Chain(parent, _Concerns, (p) => new WebsocketHandler(p, handler, _SupportedProtocols));
    }

    #endregion

}
