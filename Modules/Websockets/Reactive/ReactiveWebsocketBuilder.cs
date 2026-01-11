using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Websockets.Provider;

namespace GenHTTP.Modules.Websockets.Reactive;

public class ReactiveWebsocketBuilder : WebsocketBuilder<ReactiveWebsocketBuilder>
{
    private Func<IRequest, IReactiveHandler>? _handlerFactory;

    /// <summary>
    /// Sets the handler to react to incoming message frames.
    /// </summary>
    /// <param name="handler">The handler to react to incoming message frames.</param>
    public ReactiveWebsocketBuilder Handler(IReactiveHandler handler)
        => Handler((_) => handler);

    /// <summary>
    /// Sets the handler to react to incoming message frames (created per connection).
    /// </summary>
    /// <typeparam name="T">The type of handler to be used</typeparam>
    public ReactiveWebsocketBuilder Handler<T>() where T : IReactiveHandler, new()
        => Handler((_) => new T());

    /// <summary>
    /// Sets the handler factory to react to incoming message frames.
    /// </summary>
    /// <param name="handler">The handler factory to react to incoming message frames.</param>
    public ReactiveWebsocketBuilder Handler(Func<IRequest, IReactiveHandler> handlerFactory)
    {
        _handlerFactory = handlerFactory;
        return this;
    }

    public override IHandler Build()
    {
        if (_handlerFactory == null)
        {
            throw new BuilderMissingPropertyException("HandlerFactory");
        }

        return Concerns.Chain(_concerns, new WebsocketHandler(ContentFactory));

        ReactiveWebsocketContent ContentFactory(IRequest r) => new(_handlerFactory, r, BuildSettings());
    }

}
