using GenHTTP.Modules.DependencyInjection.Infrastructure;
using GenHTTP.Modules.Websockets;
using GenHTTP.Modules.Websockets.Imperative;
using GenHTTP.Modules.Websockets.Reactive;

namespace GenHTTP.Modules.DependencyInjection;

public static class DependentWebsocket
{

    /// <summary>
    /// Instructs the websocket handler to resolve the given type and all of its dependencies
    /// from the service scope used by the server to handle incoming requests.
    /// </summary>
    /// <typeparam name="T">The type of handler to be resolved</typeparam>
    public static ImperativeWebsocketBuilder DependentHandler<T>(this ImperativeWebsocketBuilder builder) where T : IImperativeHandler
        => builder.Handler(r => HandlerResolver.Obtain<T>(r));

    /// <summary>
    /// Instructs the websocket handler to resolve the given type and all of its dependencies
    /// from the service scope used by the server to handle incoming requests.
    /// </summary>
    /// <typeparam name="T">The type of handler to be resolved</typeparam>
    public static ReactiveWebsocketBuilder DependentHandler<T>(this ReactiveWebsocketBuilder builder) where T : IReactiveHandler
        => builder.Handler(r => HandlerResolver.Obtain<T>(r));

}
