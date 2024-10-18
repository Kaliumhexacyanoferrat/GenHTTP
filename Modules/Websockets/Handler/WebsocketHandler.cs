using Fleck;

using GenHTTP.Api.Protocol;

using IHandler = GenHTTP.Api.Content.IHandler;

namespace GenHTTP.Modules.Websockets.Handler;

public sealed class WebsocketHandler : IHandler
{

    public IHandler Parent { get; }

    public Action<IWebSocketConnection> Handler { get; }

    public List<string> SupportedProtocols { get; }

    public WebsocketHandler(IHandler parent, Action<IWebSocketConnection> handler, List<string> supportedProtocols)
    {
        Parent = parent;
        Handler = handler;
        SupportedProtocols = supportedProtocols;
    }

    public ValueTask PrepareAsync() => new();

    public ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var upgrade = request.Upgrade();

        var connection = new WebsocketConnection(upgrade.Socket, SupportedProtocols, Handler);

        connection.Start(request);

        return new(upgrade.Response);
    }

}
