using GenHTTP.Api.Content;
using GenHTTP.Modules.Conversion.Serializers;

namespace GenHTTP.Modules.Websockets.Provider;

public abstract class WebsocketBuilder<T> : IHandlerBuilder<T> where T : WebsocketBuilder<T>
{
    protected readonly List<IConcernBuilder> _concerns = [];

    protected bool _handleContinuationFramesManually, _allocateFrameData = true;

    protected int _maxRxBufferSize = 1024 * 16; // 16 KB

    protected ISerializationFormat? _serializationFormat;

    public T Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);
        return (T)this;
    }

    /// <summary>
    /// Sets the serialization handler to send and receive structured data.
    /// </summary>
    /// <param name="format">The format to use for this socket</param>
    public T Serialization(ISerializationFormat? format)
    {
        _serializationFormat = format;
        return (T)this;
    }

    /// <summary>
    /// Sets the size a websocket frame received from the
    /// server can be at maximum. Defaults to 16 KB.
    /// </summary>
    /// <param name="maxRxBufferSize">The maximum read size for websocket frames</param>
    public T MaxFrameSize(int maxRxBufferSize)
    {
        _maxRxBufferSize = maxRxBufferSize;
        return (T)this;
    }

    /// <summary>
    /// By default, messages split into multiple web socket frames will be transparently
    /// handled and aggregated by the frame readers. If you would like to handle
    /// such frames manually, you can set this option.
    /// </summary>
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

    protected ConnectionSettings BuildSettings() => new(_serializationFormat, _maxRxBufferSize, _handleContinuationFramesManually, _allocateFrameData);

    public abstract IHandler Build();

}
