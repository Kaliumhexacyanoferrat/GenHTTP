using GenHTTP.Api.Content;
using GenHTTP.Modules.Conversion.Formatters;
using GenHTTP.Modules.Conversion.Serializers;
using GenHTTP.Modules.Websockets.Protocol;
using GenHTTP.Modules.Websockets.Reactive;

namespace GenHTTP.Modules.Websockets.Functional;

public class FunctionalWebsocketBuilder : IHandlerBuilder<FunctionalWebsocketBuilder>
{
    private readonly ReactiveWebsocketBuilder _builder = Websocket.Reactive();

    private Func<IReactiveConnection, ValueTask> _onConnected = (_) => ValueTask.CompletedTask;

    private Func<IReactiveConnection, IWebsocketFrame, ValueTask> _onMessage = (_, __) => ValueTask.CompletedTask;
    private Func<IReactiveConnection, IWebsocketFrame, ValueTask> _onBinary = (_, __) => ValueTask.CompletedTask;
    private Func<IReactiveConnection, IWebsocketFrame, ValueTask> _onContinue = (_, __) => ValueTask.CompletedTask;
    private Func<IReactiveConnection, IWebsocketFrame, ValueTask> _onPing = (c, m) => c.PongAsync(m.Data);
    private Func<IReactiveConnection, IWebsocketFrame, ValueTask> _onPong = (_, __) => ValueTask.CompletedTask;
    private Func<IReactiveConnection, IWebsocketFrame, ValueTask> _onClose = (c, __) => c.CloseAsync();

    private Func<IReactiveConnection, FrameError, ValueTask<bool>> _onError = (_, __) => ValueTask.FromResult(true);

    public FunctionalWebsocketBuilder Add(IConcernBuilder concern)
    {
        _builder.Add(concern);
        return this;
    }

    public FunctionalWebsocketBuilder Formatters(FormatterRegistry formatters)
    {
        _builder.Formatters(formatters);
        return this;
    }

    public FunctionalWebsocketBuilder Serialization(ISerializationFormat format)
    {
        _builder.Serialization(format);
        return this;
    }

    public FunctionalWebsocketBuilder MaxFrameSize(int maxRxBufferSize)
    {
        _builder.MaxFrameSize(maxRxBufferSize);
        return this;
    }

    public FunctionalWebsocketBuilder HandleContinuationFramesManually()
    {
        _builder.HandleContinuationFramesManually();
        return this;
    }

    public FunctionalWebsocketBuilder DoNotAllocateFrameData()
    {
        _builder.DoNotAllocateFrameData();
        return this;
    }

    /// <summary>
    /// Invoked when a new client has connected.
    /// </summary>
    /// <param name="connection">The connection to the client</param>
    public FunctionalWebsocketBuilder OnConnected(Func<IReactiveConnection, ValueTask> handler)
    {
        _onConnected = handler;
        return this;
    }

    /// <summary>
    /// Invoked when a message frame has been received.
    /// </summary>
    /// <param name="connection">The connection to the client</param>
    /// <param name="message">The incoming message frame</param>
    public FunctionalWebsocketBuilder OnMessage(Func<IReactiveConnection, IWebsocketFrame, ValueTask> handler)
    {
        _onMessage = handler;
        return this;
    }

    /// <summary>
    /// Invoked when a binary message frame has been received.
    /// </summary>
    /// <param name="connection">The connection to the client</param>
    /// <param name="message">The incoming binary message frame</param>
    public FunctionalWebsocketBuilder OnBinary(Func<IReactiveConnection, IWebsocketFrame, ValueTask> handler)
    {
        _onBinary = handler;
        return this;
    }

    /// <summary>
    /// Invoked when the client splits frames into multiple messages
    /// and an additional frame after the first has been received.
    /// </summary>
    /// <param name="connection">The connection to the client</param>
    /// <param name="message">The incoming message frame</param>
    /// <remarks>
    /// If fragmentation occurs, you will receive a message frame with
    /// fin set to false. After this message, you will receive additional
    /// messages until one finally has fin set to true. From those frames,
    /// you have to construct the overall message originally sent by the client.
    /// </remarks>
    public FunctionalWebsocketBuilder OnContinue(Func<IReactiveConnection, IWebsocketFrame, ValueTask> handler)
    {
        _onContinue = handler;
        return this;
    }

    /// <summary>
    /// Invoked when a ping frame has been received.
    /// </summary>
    /// <param name="connection">The connection to the client</param>
    /// <param name="message">The incoming ping frame</param>
    public FunctionalWebsocketBuilder OnPing(Func<IReactiveConnection, IWebsocketFrame, ValueTask> handler)
    {
        _onPing = handler;
        return this;
    }

    /// <summary>
    /// Invoked when a pong frame has been received.
    /// </summary>
    /// <param name="connection">The connection to the client</param>
    /// <param name="message">The incoming pong frame</param>
    public FunctionalWebsocketBuilder OnPong(Func<IReactiveConnection, IWebsocketFrame, ValueTask> handler)
    {
        _onPong = handler;
        return this;
    }

    /// <summary>
    /// Invoked when the client requests to close the connection.
    /// </summary>
    /// <param name="connection">The connection to the client</param>
    /// <param name="message">The frame sent by the client</param>
    public FunctionalWebsocketBuilder OnClose(Func<IReactiveConnection, IWebsocketFrame, ValueTask> handler)
    {
        _onClose = handler;
        return this;
    }

    /// <summary>
    /// Invoked if the server failed to read a frame from the connection.
    /// </summary>
    /// <param name="connection">The connection to the client</param>
    /// <param name="error">The error which ocurred</param>
    /// <returns>true, if the connection should be closed</returns>
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
