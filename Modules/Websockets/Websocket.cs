using GenHTTP.Modules.Websockets.Functional;
using GenHTTP.Modules.Websockets.Imperative;
using GenHTTP.Modules.Websockets.Legacy;
using GenHTTP.Modules.Websockets.Reactive;

namespace GenHTTP.Modules.Websockets;

/// <summary>
/// Provides handlers that allow you to process a web socket connection.
/// There are several flavors available, depending on your personal
/// preference and the nature of the problem you would like to solve.
/// </summary>
public static class Websocket
{

    /// <summary>
    /// Creates a new builder to configure a websocket handler
    /// that will process incoming websocket requests.
    /// </summary>
    [Obsolete("The web socket implementation based on Fleck will be removed with GenHTTP 11.")]
    public static WebsocketHandlerBuilder Create() => new();

    /// <summary>
    /// Creates a web socket that allows you to read
    /// messages one-by-one from the socket connection and handle
    /// them as needed, typically in a loop.
    /// </summary>
    public static ImperativeWebsocketBuilder Imperative() => new();

    /// <summary>
    /// Creates a web socket that allows you to specify a handler
    /// implementation that reacts to specific events, such as
    /// a new message being received.
    /// </summary>
    /// <remarks>
    /// In contrast to the imperative web socket, this socket will
    /// provide a message pump that will read incoming messages
    /// and dispatch them to your handler.
    /// </remarks>
    public static ReactiveWebsocketBuilder Reactive() => new();

    /// <summary>
    /// Creates a web socket that allows you to specify delegates
    /// for the type of messages you would like to handle.
    /// </summary>
    /// <remarks>
    /// In contrast to the reactive web socket, this builder allows
    /// you to directly handle events without the need of implementing
    /// a handler interface.
    /// </remarks>
    public static FunctionalWebsocketBuilder Functional() => new();

}
