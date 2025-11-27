using GenHTTP.Api.Content;
using GenHTTP.Modules.Straculo.Contents;

namespace GenHTTP.Modules.Straculo.Provider;

public class MulticastWebsocketBuilder : IHandlerBuilder<MulticastWebsocketBuilder>
{
    private readonly List<IConcernBuilder> _concerns = [];

    private readonly MulticastWebsocketContent _multicastWebsocketContent;

    public MulticastWebsocketBuilder()
    {
        _multicastWebsocketContent = new MulticastWebsocketContent();
    }
    
    public MulticastWebsocketBuilder OnConnected(Func<Stream, ValueTask> onConnected)
    {
        _multicastWebsocketContent.OnConnected = onConnected;
        return this;
    }
    
    public MulticastWebsocketBuilder OnMessage(Func<Stream, ValueTask> onMessage)
    {
        _multicastWebsocketContent.OnMessage = onMessage;
        return this;
    }
    
    public MulticastWebsocketBuilder OnBinary(Func<Stream, ValueTask> onBinary)
    {
        _multicastWebsocketContent.OnBinary = onBinary;
        return this;
    }
    
    public MulticastWebsocketBuilder OnContinue(Func<Stream, ValueTask> onContinue)
    {
        _multicastWebsocketContent.OnContinue = onContinue;
        return this;
    }
    
    public MulticastWebsocketBuilder OnPing(Func<Stream, ValueTask> onPing)
    {
        _multicastWebsocketContent.OnPing = onPing;
        return this;
    }
    
    public MulticastWebsocketBuilder OnPong(Func<Stream, ValueTask> onPong)
    {
        _multicastWebsocketContent.OnPong = onPong;
        return this;
    }
    
    public MulticastWebsocketBuilder OnClose(Func<Stream, ValueTask> onClose)
    {
        _multicastWebsocketContent.OnClose = onClose;
        return this;
    }
    
    public MulticastWebsocketBuilder Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);
        return this;
    }

    public IHandler Build()
    {
        return Concerns.Chain(_concerns, new WebsocketProvider(_multicastWebsocketContent));
    }
}