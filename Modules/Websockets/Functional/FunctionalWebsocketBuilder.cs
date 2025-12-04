using System.Net.WebSockets;
using GenHTTP.Api.Content;
using GenHTTP.Modules.Websockets.Protocol;
using GenHTTP.Modules.Websockets.Reactive;

namespace GenHTTP.Modules.Websockets.Functional;

public class FunctionalWebsocketBuilder : IHandlerBuilder<FunctionalWebsocketBuilder>
{
    private readonly ReactiveWebsocketBuilder _builder = Websocket.Reactive();

    private Func<IReactiveConnection, ValueTask> _onConnected = (_) => ValueTask.CompletedTask;

    private Func<IReactiveConnection, WebsocketFrame, ValueTask> _onMessage = (_, __) => ValueTask.CompletedTask;
    private Func<IReactiveConnection, WebsocketFrame, ValueTask> _onBinary = (_, __) => ValueTask.CompletedTask;
    private Func<IReactiveConnection, WebsocketFrame, ValueTask> _onContinue = (_, __) => ValueTask.CompletedTask;
    private Func<IReactiveConnection, WebsocketFrame, ValueTask> _onPing = (_, __) => ValueTask.CompletedTask;
    private Func<IReactiveConnection, WebsocketFrame, ValueTask> _onPong = (_, __) => ValueTask.CompletedTask;
    private Func<IReactiveConnection, WebsocketFrame, ValueTask> _onClose = (_, __) => ValueTask.CompletedTask;

    private Func<IReactiveConnection, FrameError, ValueTask<bool>> _onError = (_, __) => ValueTask.FromResult(true);

    public FunctionalWebsocketBuilder Add(IConcernBuilder concern)
    {
        _builder.Add(concern);
        return this;
    }

    public FunctionalWebsocketBuilder OnConnected(Func<IReactiveConnection, ValueTask> handler)
    {
        _onConnected = handler;
        return this;
    }

    public FunctionalWebsocketBuilder OnMessage(Func<IReactiveConnection, WebsocketFrame, ValueTask> handler)
    {
        _onMessage = handler;
        return this;
    }

    public FunctionalWebsocketBuilder OnBinary(Func<IReactiveConnection, WebsocketFrame, ValueTask> handler)
    {
        _onBinary = handler;
        return this;
    }

    public FunctionalWebsocketBuilder OnContinue(Func<IReactiveConnection, WebsocketFrame, ValueTask> handler)
    {
        _onContinue = handler;
        return this;
    }

    public FunctionalWebsocketBuilder OnPing(Func<IReactiveConnection, WebsocketFrame, ValueTask> handler)
    {
        _onPing = handler;
        return this;
    }

    public FunctionalWebsocketBuilder OnPong(Func<IReactiveConnection, WebsocketFrame, ValueTask> handler)
    {
        _onPong = handler;
        return this;
    }

    public FunctionalWebsocketBuilder OnClose(Func<IReactiveConnection, WebsocketFrame, ValueTask> handler)
    {
        _onClose = handler;
        return this;
    }

    public FunctionalWebsocketBuilder OnError(Func<IReactiveConnection, FrameError, ValueTask<bool>> handler)
    {
        _onError = handler;
        return this;
    }

    public IHandler Build()
    {
        var handler = new FunctionalHandler(_onConnected, _onMessage, _onBinary, _onContinue, _onPing, _onPong, _onClose, _onError);

        _builder.Handler(handler);

        return _builder.Build();
    }

}
