using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Websockets.Provider;

namespace GenHTTP.Modules.Websockets.Reactive;

public class ReactiveWebsocketBuilder : WebsocketBuilder<ReactiveWebsocketBuilder>
{
    private IReactiveHandler? _handler;

    /// <summary>
    /// Sets the handler to react to incoming message frames.
    /// </summary>
    /// <param name="handler">The handler to react to incoming message frames.</param>
    public ReactiveWebsocketBuilder Handler(IReactiveHandler handler)
    {
        _handler = handler;
        return this;
    }

    public override IHandler Build()
    {
        if (_handler == null)
        {
            throw new BuilderMissingPropertyException("Handler");
        }

        var contentFactory = (IRequest r) => new ReactiveWebsocketContent(_handler, r, _maxRxBufferSize, _handleContinuationFramesManually);

        return Concerns.Chain(_concerns, new WebsocketHandler(contentFactory));
    }

}
