using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Websockets.Provider;

namespace GenHTTP.Modules.Websockets.Imperative;

public class ImperativeWebsocketBuilder : WebsocketBuilder<ImperativeWebsocketBuilder>
{
    private Func<IRequest, IImperativeHandler>? _handlerFactory;

    /// <summary>
    /// Sets the handler used to establish a message pump.
    /// </summary>
    /// <param name="handler">The handler used to establish a message pump</param>
    public ImperativeWebsocketBuilder Handler(IImperativeHandler handler)
        => Handler((_) => handler);

    /// <summary>
    /// Sets the handler used to establish a message pump (created per connection).
    /// </summary>
    /// <typeparam name="T">The type of handler to be used</typeparam>
    public ImperativeWebsocketBuilder Handler<T>() where T : IImperativeHandler, new()
        => Handler((_) => new T());

    /// <summary>
    /// Sets the handler factory used to establish a message pump.
    /// </summary>
    /// <param name="handler">The handler factory used to establish a message pump</param>
    public ImperativeWebsocketBuilder Handler(Func<IRequest, IImperativeHandler> handlerFactory)
    {
        _handlerFactory = handlerFactory;
        return this;
    }

    public override IHandler Build()
    {
        if (_handlerFactory is null)
        {
            throw new BuilderMissingPropertyException("HandlerFactory");
        }

        var contentFactory = (IRequest r) => new ImperativeWebsocketContent(_handlerFactory, r, BuildSettings());

        return Concerns.Chain(_concerns, new WebsocketHandler(contentFactory));
    }

}
