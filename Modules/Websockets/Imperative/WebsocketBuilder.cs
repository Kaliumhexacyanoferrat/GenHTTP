using GenHTTP.Api.Content;
using GenHTTP.Modules.Websockets.Provider;

namespace GenHTTP.Modules.Websockets.Imperative;

public class WebsocketBuilder : IHandlerBuilder<WebsocketBuilder>
{
    private readonly List<IConcernBuilder> _concerns = [];

    private WebsocketContent? _websocketContent;

    public WebsocketBuilder Add(WebsocketContent websocketContent)
    {
        _websocketContent =  websocketContent;
        return this;
    }
    
    public WebsocketBuilder Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);
        return this;
    }

    public IHandler Build()
    {
        if (_websocketContent is null)
        {
            throw new InvalidOperationException("Websocket content is not initialized");
        }
        
        return Concerns.Chain(_concerns, new WebsocketProvider(_websocketContent));
    }
}