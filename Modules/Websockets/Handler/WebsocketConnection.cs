using System.Net.Sockets;

using Fleck;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Websockets.Handler;

public sealed class WebsocketConnection : IWebSocketConnection, IWebsocketConnection
{
    private const int ReadSize = 1024 * 4;

    private bool _Closing;

    private bool _Closed;

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

    public bool IsAvailable
    {
        get { return !_Closing && !_Closed && Socket.Connected; }
    }

    #endregion

    #region Initialization

    public WebsocketConnection(Socket socket, IRequest request, List<string> supportedProtocols,
        Action<IWebsocketConnection>? onOpen,
        Action<IWebsocketConnection>? onClose,
        Action<IWebsocketConnection, string>? onMessage,
        Action<IWebsocketConnection, byte[]>? onBinary,
        Action<IWebsocketConnection, byte[]>? onPing,
        Action<IWebsocketConnection, byte[]>? onPong,
        Action<IWebsocketConnection, Exception>? onError)
    {
        Socket = new SocketWrapper(socket);
        Request = request;

        SupportedProtocols = supportedProtocols;
        
        OnOpen = (onOpen != null) ? () => onOpen(this) : () => { };
        OnClose = (onClose != null) ? () => onClose(this) : () => { };
        OnMessage = (onMessage != null) ? (x) => onMessage(this, x) : x => { };
        OnBinary = (onBinary != null) ? (x) => onBinary(this, x) : x => { };
        OnPing = (onPing != null) ? (x) => onPing(this, x) : x => SendPong(x);
        OnPong = (onPong != null) ? (x) => onPong(this, x) : x => { };
        OnError = (onError != null) ? (x) => onError(this, x) : x => { };
    }

    #endregion

    #region Functionality

    public Task Send(string message)
    {
        return Send(message, GetHandler().FrameText);
    }

    public Task Send(byte[] message)
    {
        return Send(message, GetHandler().FrameBinary);
    }

    public Task SendPing(byte[] message)
    {
        return Send(message, GetHandler().FramePing);
    }

    public Task SendPong(byte[] message)
    {
        return Send(message, GetHandler().FramePong);
    }

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

        var data = new List<byte>(ReadSize);
        var buffer = new byte[ReadSize];

        Read(data, buffer);
    }

    public void Close()
    {
        Close(WebSocketStatusCodes.NormalClosure);
    }

    public void Close(int code)
    {
        if (!IsAvailable)
            return;

        _Closing = true;

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

    private void Read(List<byte> data, byte[] buffer)
    {
        if (!IsAvailable)
            return;

        Socket.Receive(buffer, r =>
                       {
                           if (r <= 0)
                           {
                               FleckLog.Debug("0 bytes read. Closing.");
                               CloseSocket();
                               return;
                           }
                           FleckLog.Debug(r + " bytes read");
                           var readBytes = buffer.Take(r);

                           GetHandler().Receive(readBytes);

                           Read(data, buffer);
                       },
                       HandleReadError);
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
        _Closing = true;
        OnClose();
        _Closed = true;
        Socket.Close();
        Socket.Dispose();
        _Closing = false;
    }

    private IHandler GetHandler() => Handler ?? throw new InvalidOperationException("Handler expected but not set");

    #endregion

}
