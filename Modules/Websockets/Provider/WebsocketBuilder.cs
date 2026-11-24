using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Modules.Conversion;
using GenHTTP.Modules.Conversion.Formatters;
using GenHTTP.Modules.Conversion.Serializers;
using GenHTTP.Modules.Conversion.Serializers.Json;

namespace GenHTTP.Modules.Websockets.Provider;

public abstract class WebsocketBuilder<T> : IHandlerBuilder<T> where T : WebsocketBuilder<T>
{
    protected readonly List<IConcernBuilder> _concerns = [];

    protected bool _handleContinuationFramesManually, _allocateFrameData = true, _synchronizeWrites = true;

    protected int _maxRxBufferSize = BufferSize.Read;

    protected FormatterRegistry _formatters = Formatting.Default().Build();

    protected ISerializationFormat _serializationFormat = new JsonFormat();

    public T Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);
        return (T)this;
    }

    /// <summary>
    /// Sets the formatting registry used to send and receive formatted data.
    /// </summary>
    /// <param name="formatters">The formatters to use for this socket</param>
    public T Formatters(FormatterRegistry formatters)
    {
        _formatters = formatters;
        return (T)this;
    }

    /// <summary>
    /// Sets the serialization handler to send and receive structured data.
    /// </summary>
    /// <param name="format">The format to use for this socket</param>
    public T Serialization(ISerializationFormat format)
    {
        _serializationFormat = format;
        return (T)this;
    }

    /// <summary>
    /// By default, messages split into multiple web socket frames will be transparently
    /// handled and aggregated by the frame readers. If you would like to handle
    /// such frames manually, you can set this option.
    /// </summary>
    /// <remarks>
    /// The frames of a fragmented text message are then passed on as they arrive, so checking
    /// that the message is valid UTF-8 is up to the handler. Unfragmented text messages are
    /// still checked.
    /// </remarks>
    public T HandleContinuationFramesManually()
    {
        _handleContinuationFramesManually = true;
        return (T)this;
    }

    /// <summary>
    /// By default, the handlers will allocate the data within a websocket frame, so the
    /// data can still be referenced after the next message has been read from the connection.
    /// </summary>
    /// <remarks>
    /// For high performance scenarios, you can disable the automatic allocation. In this case
    /// you need to ensure, that access to frame data only happens while the frame is still the
    /// latest read from the connection.
    /// </remarks>
    public T DoNotAllocateFrameData()
    {
        _allocateFrameData = false;
        return (T)this;
    }

    /// <summary>
    /// By default, a connection serializes every write and its flush, so frames can be sent from
    /// several threads at once - including the pong and close frames sent by the default
    /// <c>OnPing</c> and <c>OnClose</c> handlers, which run on the message pump.
    /// </summary>
    /// <remarks>
    /// If a connection is only ever written by one thread at a time, you can disable this to skip
    /// the lock on every write. Writes from several threads then need to be serialized by the
    /// caller, the frames of the default <c>OnPing</c> and <c>OnClose</c> handlers included.
    ///
    /// The synchronization covers one call at a time. A message split into several frames
    /// (<c>fin: false</c>) still needs the caller's lock across all of them, so another message
    /// cannot get in between.
    /// </remarks>
    public T DisableWriteSynchronization()
    {
        _synchronizeWrites = false;
        return (T)this;
    }

    /// <summary>
    /// Sets the largest payload, in bytes, a single frame sent by the client may carry (16 KiB by default).
    /// A larger frame is reported as a <see cref="Protocol.FrameErrorType.PayloadTooLarge" /> error.
    /// </summary>
    /// <remarks>
    /// A frame is kept in memory until it has been received completely. A message split into
    /// several frames may be larger than this.
    /// </remarks>
    /// <param name="bytes">The largest payload of a single frame</param>
    /// <example>
    /// <code>
    /// Websocket.Imperative().MaxFrameSize(64 * 1024).Handler(new MyHandler());
    /// </code>
    /// </example>
    public T MaxFrameSize(int bytes)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bytes);

        _maxRxBufferSize = bytes;
        return (T)this;
    }

    protected ConnectionSettings BuildSettings() => new(_formatters, _serializationFormat, _handleContinuationFramesManually, _allocateFrameData, _synchronizeWrites, _maxRxBufferSize);

    public abstract IHandler Build();

}
