using GenHTTP.Api.Content;
using GenHTTP.Modules.Straculo.Protocol;
using GenHTTP.Modules.Straculo.Provider;
using GenHTTP.Modules.Straculo.Utils;

namespace GenHTTP.Modules.Straculo.Reactive;

public class ReactiveWebsocketBuilder : IHandlerBuilder<ReactiveWebsocketBuilder>
{
    private readonly List<IConcernBuilder> _concerns = [];

    private readonly ReactiveWebsocketContent _reactiveWebsocketContent;

    public ReactiveWebsocketBuilder(int rxBufferSize)
    {
        _reactiveWebsocketContent = new ReactiveWebsocketContent(rxBufferSize);
    }
    
    public ReactiveWebsocketBuilder OnConnected(Func<WebsocketStream, ValueTask> onConnected)
    {
        _reactiveWebsocketContent.OnConnected = onConnected;
        return this;
    }
    
    public ReactiveWebsocketBuilder OnMessage(Func<WebsocketStream, WebsocketFrame, ValueTask> onMessage)
    {
        _reactiveWebsocketContent.OnMessage = onMessage;
        return this;
    }
    
    public ReactiveWebsocketBuilder OnBinary(Func<WebsocketStream, WebsocketFrame, ValueTask> onBinary)
    {
        _reactiveWebsocketContent.OnBinary = onBinary;
        return this;
    }
    
    public ReactiveWebsocketBuilder OnContinue(Func<WebsocketStream, WebsocketFrame, ValueTask> onContinue)
    {
        _reactiveWebsocketContent.OnContinue = onContinue;
        return this;
    }
    
    public ReactiveWebsocketBuilder OnPing(Func<WebsocketStream, WebsocketFrame, ValueTask> onPing)
    {
        _reactiveWebsocketContent.OnPing = onPing;
        return this;
    }
    
    public ReactiveWebsocketBuilder OnPong(Func<WebsocketStream, ValueTask> onPong)
    {
        _reactiveWebsocketContent.OnPong = onPong;
        return this;
    }
    
    public ReactiveWebsocketBuilder OnClose(Func<WebsocketStream, WebsocketFrame, ValueTask> onClose)
    {
        _reactiveWebsocketContent.OnClose = onClose;
        return this;
    }
    
    public ReactiveWebsocketBuilder OnError(Func<WebsocketStream, FrameError, ValueTask<bool>> onError)
    {
        _reactiveWebsocketContent.OnError = onError;
        return this;
    }
    
    public ReactiveWebsocketBuilder Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);
        return this;
    }

    public IHandler Build()
    {
        return Concerns.Chain(_concerns, new WebsocketProvider(_reactiveWebsocketContent));
    }
}