using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.Websockets.Provider;

namespace GenHTTP.Modules.Websockets.Imperative;

public class ImperativeWebsocketBuilder : IHandlerBuilder<ImperativeWebsocketBuilder>
{
    private readonly List<IConcernBuilder> _concerns = [];

    private IImperativeHandler? _handler;

    public ImperativeWebsocketBuilder Handler(IImperativeHandler handler)
    {
        _handler = handler;
        return this;
    }
    
    public ImperativeWebsocketBuilder Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);
        return this;
    }

    public IHandler Build()
    {
        if (_handler is null)
        {
            throw new BuilderMissingPropertyException("Handler");
        }

        var content = new ImperativeWebsocketContent(_handler);
        
        return Concerns.Chain(_concerns, new WebsocketHandler(content));
    }
    
}