using GenHTTP.Modules.Websockets.Protocol;

namespace GenHTTP.Modules.Websockets.Functional;

public class FunctionalHandler : IReactiveHandler
{
    private readonly Func<IReactiveConnection, ValueTask> _onConnected;

    private readonly Func<IReactiveConnection, IWebsocketFrame, ValueTask> _onMessage;
    private readonly Func<IReactiveConnection, IWebsocketFrame, ValueTask> _onBinary;
    private readonly Func<IReactiveConnection, IWebsocketFrame, ValueTask> _onContinue;
    private readonly Func<IReactiveConnection, IWebsocketFrame, ValueTask> _onPing;
    private readonly Func<IReactiveConnection, IWebsocketFrame, ValueTask> _onPong;
    private readonly Func<IReactiveConnection, IWebsocketFrame, ValueTask> _onClose;

    private readonly Func<IReactiveConnection, FrameError, ValueTask<bool>> _onError;

    public FunctionalHandler(Func<IReactiveConnection, ValueTask> onConnected,
        Func<IReactiveConnection, IWebsocketFrame, ValueTask> onMessage,
        Func<IReactiveConnection, IWebsocketFrame, ValueTask> onBinary,
        Func<IReactiveConnection, IWebsocketFrame, ValueTask> onContinue,
        Func<IReactiveConnection, IWebsocketFrame, ValueTask> onPing,
        Func<IReactiveConnection, IWebsocketFrame, ValueTask> onPong,
        Func<IReactiveConnection, IWebsocketFrame, ValueTask> onClose,
        Func<IReactiveConnection, FrameError, ValueTask<bool>> onError)
    {
        _onConnected = onConnected;

        _onMessage = onMessage;
        _onBinary = onBinary;
        _onContinue = onContinue;
        _onPing = onPing;
        _onPong = onPong;
        _onClose = onClose;

        _onError = onError;
    }

    public ValueTask OnConnected(IReactiveConnection connection) => _onConnected(connection);

    public ValueTask OnMessage(IReactiveConnection connection, IWebsocketFrame message) => _onMessage(connection, message);

    public ValueTask OnBinary(IReactiveConnection connection, IWebsocketFrame message) => _onBinary(connection, message);

    public ValueTask OnContinue(IReactiveConnection connection, IWebsocketFrame message) => _onContinue(connection, message);

    public ValueTask OnPing(IReactiveConnection connection, IWebsocketFrame message) => _onPing(connection, message);

    public ValueTask OnPong(IReactiveConnection connection, IWebsocketFrame message) => _onPong(connection, message);

    public ValueTask OnClose(IReactiveConnection connection, IWebsocketFrame message) =>  _onClose(connection, message);

    public ValueTask<bool> OnError(IReactiveConnection connection, FrameError error) => _onError(connection, error);

}
