using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Websockets.Provider;

namespace GenHTTP.Modules.Websockets.Reactive;

public class ReactiveWebsocketBuilder : IHandlerBuilder<ReactiveWebsocketBuilder>
{
    private readonly List<IConcernBuilder> _concerns = [];

    private int _maxRxBufferSize = 8192;

    private IReactiveHandler? _handler;

    public ReactiveWebsocketBuilder Handler(IReactiveHandler handler)
    {
        _handler = handler;
        return this;
    }

    public ReactiveWebsocketBuilder Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);
        return this;
    }

    public ReactiveWebsocketBuilder MaxFrameSize(int maxRxBufferSize)
    {
        _maxRxBufferSize = maxRxBufferSize;
        return this;
    }

    public IHandler Build()
    {
        if (_handler == null)
        {
            throw new BuilderMissingPropertyException("Handler");
        }

        var contentFactory = (IRequest r) => new ReactiveWebsocketContent(_handler, r, _maxRxBufferSize);

        return Concerns.Chain(_concerns, new WebsocketHandler(contentFactory));
    }

}
