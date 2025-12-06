using GenHTTP.Modules.Websockets.Protocol;

namespace GenHTTP.Modules.Websockets.Functional;

public class FunctionalHandler : IReactiveHandler
{
    private readonly Func<IReactiveConnection, ValueTask> _onConnected;

    private readonly Func<IReactiveConnection, WebsocketFrame, ValueTask> _onMessage;
    private readonly Func<IReactiveConnection, WebsocketFrame, ValueTask> _onBinary;
    private readonly Func<IReactiveConnection, WebsocketFrame, ValueTask> _onContinue;
    private readonly Func<IReactiveConnection, WebsocketFrame, ValueTask> _onPing;
    private readonly Func<IReactiveConnection, WebsocketFrame, ValueTask> _onPong;
    private readonly Func<IReactiveConnection, WebsocketFrame, ValueTask> _onClose;

    private readonly Func<IReactiveConnection, FrameError, ValueTask<bool>> _onError;

    public FunctionalHandler(Func<IReactiveConnection, ValueTask> onConnected,
        Func<IReactiveConnection, WebsocketFrame, ValueTask> onMessage,
        Func<IReactiveConnection, WebsocketFrame, ValueTask> onBinary,
        Func<IReactiveConnection, WebsocketFrame, ValueTask> onContinue,
        Func<IReactiveConnection, WebsocketFrame, ValueTask> onPing,
        Func<IReactiveConnection, WebsocketFrame, ValueTask> onPong,
        Func<IReactiveConnection, WebsocketFrame, ValueTask> onClose,
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

    public ValueTask OnMessage(IReactiveConnection connection, WebsocketFrame message) => _onMessage(connection, message);

    public ValueTask OnBinary(IReactiveConnection connection, WebsocketFrame message) => _onBinary(connection, message);

    public ValueTask OnContinue(IReactiveConnection connection, WebsocketFrame message) => _onContinue(connection, message);

    public ValueTask OnPing(IReactiveConnection connection, WebsocketFrame message) => _onPing(connection, message);

    public ValueTask OnPong(IReactiveConnection connection, WebsocketFrame message) => _onPong(connection, message);

    public ValueTask OnClose(IReactiveConnection connection, WebsocketFrame message) =>  _onClose(connection, message);

    public ValueTask<bool> OnError(IReactiveConnection connection, FrameError error) => _onError(connection, error);


}
