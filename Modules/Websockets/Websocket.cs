using GenHTTP.Modules.Websockets.Imperative;
using GenHTTP.Modules.Websockets.Legacy;
using GenHTTP.Modules.Websockets.Reactive;

namespace GenHTTP.Modules.Websockets;

public static class Websocket
{

    /// <summary>
    /// Creates a new builder to configure a websocket handler
    /// that will process incoming websocket requests.
    /// </summary>
    [Obsolete("The web socket implementation based on Fleck will be removed with GenHTTP 11.")]
    public static WebsocketHandlerBuilder Create() => new();

    public static ImperativeWebsocketBuilder CreateImperative() => new();

    public static ReactiveWebsocketBuilder CreateReactive(int rxBufferSize = 8192) => new(rxBufferSize);

}
