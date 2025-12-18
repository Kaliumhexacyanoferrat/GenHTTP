using GenHTTP.Api.Content;

namespace GenHTTP.Modules.Websockets.Provider;

public abstract class WebsocketBuilder<T> : IHandlerBuilder<T> where T : WebsocketBuilder<T>
{
    protected bool _handleContinuationFramesManually;
    protected bool _allocateFrameData = true;
    
    protected readonly List<IConcernBuilder> _concerns = [];

    protected int _maxRxBufferSize = 1024 * 16; // 16 KB

    public T Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);
        return (T)this;
    }
    
    public T MaxFrameSize(int maxRxBufferSize)
    {
        _maxRxBufferSize = maxRxBufferSize;
        return (T)this;
    }
    
    public T HandleContinuationFramesManually()
    {
        _handleContinuationFramesManually = true;
        return (T)this;
    }
    
    public T DoNotAllocateFrameData()
    {
        _allocateFrameData = false;
        return (T)this;
    }

    public abstract IHandler Build();

}
