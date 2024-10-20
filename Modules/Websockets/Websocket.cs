using GenHTTP.Modules.Websockets.Handler;

namespace GenHTTP.Modules.Websockets;

public static class Websocket
{

    /// <summary>
    /// Creates a new builder to configure a websocket handler
    /// that will process incoming websocket requests.
    /// </summary>
    public static WebsocketHandlerBuilder Create() => new();

}
