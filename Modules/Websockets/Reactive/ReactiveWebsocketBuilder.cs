using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Modules.Websockets.Provider;

namespace GenHTTP.Modules.Websockets.Reactive;

public class ReactiveWebsocketBuilder : IHandlerBuilder<ReactiveWebsocketBuilder>
{
    private readonly List<IConcernBuilder> _concerns = [];

    private readonly int _rxBufferSize;

    private IReactiveHandler? _handler;

    public ReactiveWebsocketBuilder(int rxBufferSize)
    {
        _rxBufferSize = rxBufferSize;
    }

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

    public IHandler Build()
    {
        if (_handler == null)
        {
            throw new BuilderMissingPropertyException("Handler");
        }

        var content = new ReactiveWebsocketContent(_handler, _rxBufferSize);
        
        return Concerns.Chain(_concerns, new WebsocketHandler(content));
    }
    
}