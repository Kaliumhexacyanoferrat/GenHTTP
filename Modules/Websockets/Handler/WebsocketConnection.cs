using Fleck;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Websockets.Handler;

public sealed class WebsocketConnection : IWebSocketConnection, IWebsocketConnection
{
    private const int ReadSize = 1024 * 4;

    private bool _closing;

    private bool _closed;

    private Task? _readingTask;

    #region Get-/Setters

    public ISocket Socket { get; }

    public IHandler? Handler { get; private set; }

    public IRequest Request { get; }

    public IWebSocketConnectionInfo? ConnectionInfo { get; private set; }

    public Action OnOpen { get; set; }

    public Action OnClose { get; set; }

    public Action<string> OnMessage { get; set; }

    public Action<byte[]> OnBinary { get; set; }

    public Action<byte[]> OnPing { get; set; }

    public Action<byte[]> OnPong { get; set; }

    public Action<Exception> OnError { get; set; }

    public List<string> SupportedProtocols { get; }

    public bool IsAvailable => !_closing && !_closed && Socket.Connected;

    #endregion

    #region Initialization

    public WebsocketConnection(ISocket socket, IRequest request, List<string> supportedProtocols,
        Func<IWebsocketConnection, Task>? onOpen,
        Func<IWebsocketConnection, Task>? onClose,
        Func<IWebsocketConnection, string, Task>? onMessage,
        Func<IWebsocketConnection, byte[], Task>? onBinary,
        Func<IWebsocketConnection, byte[], Task>? onPing,
        Func<IWebsocketConnection, byte[], Task>? onPong,
        Func<IWebsocketConnection, Exception, Task>? onError)
    {
        Socket = socket;
        Request = request;

        SupportedProtocols = supportedProtocols;

        OnOpen = (onOpen != null) ? () => WebsocketDispatcher.Schedule(() => onOpen(this)) : () => { };
        OnClose = (onClose != null) ? () => WebsocketDispatcher.Schedule(() => onClose(this)) : () => { };
        OnMessage = (onMessage != null) ? x => WebsocketDispatcher.Schedule(() => onMessage(this, x)) : x => { };
        OnBinary = (onBinary != null) ? x => WebsocketDispatcher.Schedule(() => onBinary(this, x)) : x => { };
        OnPing = (onPing != null) ? x => WebsocketDispatcher.Schedule(() => onPing(this, x)) : x => WebsocketDispatcher.Schedule(() => SendPongAsync(x));
        OnPong = (onPong != null) ? x => WebsocketDispatcher.Schedule(() => onPong(this, x)) : x => { };
        OnError = (onError != null) ? x => WebsocketDispatcher.Schedule(() => onError(this, x)) : x => { };
    }

    #endregion

    #region Functionality

    public Task Send(string message) => SendAsync(message);

    public Task Send(byte[] message) => SendAsync(message);

    public Task SendPing(byte[] message) => SendPingAsync(message);

    public Task SendPong(byte[] message) => SendPongAsync(message);

    public Task SendAsync(string message) => Send(message, GetHandler().FrameText);

    public Task SendAsync(byte[] message) => Send(message, GetHandler().FrameBinary);

    public Task SendPingAsync(byte[] message) => Send(message, GetHandler().FramePing);

    public Task SendPongAsync(byte[] message) => Send(message, GetHandler().FramePong);

    private Task Send<T>(T message, Func<T, byte[]> createFrame)
    {
        if (Handler == null)
            throw new InvalidOperationException("Cannot send before handshake");

        if (!IsAvailable)
        {
            const string errorMessage = "Data sent while closing or after close. Ignoring.";
            FleckLog.Warn(errorMessage);

            var taskForException = new TaskCompletionSource<object>();
            taskForException.SetException(new ConnectionNotAvailableException(errorMessage));
            return taskForException.Task;
        }

        var bytes = createFrame(message);
        
        return SendBytes(bytes);
    }

    public void Start()
    {
        var mappedRequest = Request.Map();

        Handler = HandlerFactory.BuildHandler(mappedRequest, OnMessage, OnClose, OnBinary, OnPing, OnPong);

        var subProtocol = SubProtocolNegotiator.Negotiate(SupportedProtocols, mappedRequest.SubProtocols);
        ConnectionInfo = WebSocketConnectionInfo.Create(mappedRequest, Socket.RemoteIpAddress, Socket.RemotePort, subProtocol);

        var handshake = Handler.CreateHandshake(subProtocol);
        SendBytes(handshake, OnOpen);

        _readingTask = StartReading();
    }

    public void Close()
    {
        Close(WebSocketStatusCodes.NormalClosure);
    }

    public void Close(int code)
    {
        if (!IsAvailable)
            return;

        _closing = true;

        if (Handler == null)
        {
            CloseSocket();
            return;
        }

        var bytes = Handler.FrameClose(code);

        if (bytes.Length == 0)
            CloseSocket();
        else
            SendBytes(bytes, CloseSocket);
    }

    private Task StartReading()
    {
        return Task.Run(async () =>
        {
            var buffer = new byte[ReadSize];

            var handler = GetHandler();

            while (IsAvailable)
            {
                var read = await Socket.Receive(buffer, _ => { }, HandleReadError);

                if (!IsAvailable) return;

                if (read <= 0)
                {
                    FleckLog.Debug("0 bytes read. Closing.");
                    CloseSocket();
                    return;
                }

                FleckLog.Debug(read + " bytes read");

                var readBytes = buffer.Take(read);

                handler.Receive(readBytes);
            }
        });
    }

    private void HandleReadError(Exception e)
    {
        if (e is AggregateException agg)
        {
            if (agg.InnerException != null)
            {
                HandleReadError(agg.InnerException);
            }

            return;
        }

        if (e is ObjectDisposedException)
        {
            FleckLog.Debug("Swallowing ObjectDisposedException", e);
            return;
        }

        OnError(e);

        if (e is WebSocketException exception)
        {
            FleckLog.Debug("Error while reading", exception);
            Close(exception.StatusCode);
        }
        else if (e is SubProtocolNegotiationFailureException)
        {
            FleckLog.Debug(e.Message);
            Close(WebSocketStatusCodes.ProtocolError);
        }
        else if (e is IOException)
        {
            FleckLog.Debug("Error while reading", e);
            Close(WebSocketStatusCodes.AbnormalClosure);
        }
        else
        {
            FleckLog.Error("Application Error", e);
            Close(WebSocketStatusCodes.InternalServerError);
        }
    }

    private Task SendBytes(byte[] bytes, Action? callback = null)
    {
        return Socket.Send(bytes, () =>
                           {
                               FleckLog.Debug("Sent " + bytes.Length + " bytes");
                               callback?.Invoke();
                           },
                           e =>
                           {
                               if (e is IOException)
                                   FleckLog.Debug("Failed to send. Disconnecting.", e);
                               else
                                   FleckLog.Info("Failed to send. Disconnecting.", e);
                               CloseSocket();
                           });
    }

    private void CloseSocket()
    {
        if (_readingTask != null)
        {
            _readingTask.Dispose();
            _readingTask = null;
        }

        _closing = true;
        OnClose();
        _closed = true;
        Socket.Close();
        Socket.Dispose();
        _closing = false;
    }

    private IHandler GetHandler() => Handler ?? throw new InvalidOperationException("Handler expected but not set");

    #endregion

}
