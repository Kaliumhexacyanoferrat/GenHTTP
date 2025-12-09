using GenHTTP.Api.Content;

namespace GenHTTP.Modules.Websockets.Provider;

public abstract class WebsocketBuilder<T> : IHandlerBuilder<T> where T : WebsocketBuilder<T>
{
    protected readonly List<IConcernBuilder> _concerns = [];

    protected int _maxRxBufferSize = 8192;

    public T MaxFrameSize(int maxRxBufferSize)
    {
        _maxRxBufferSize = maxRxBufferSize;
        return (T)this;
    }

    public T Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);
        return (T)this;
    }

    public abstract IHandler Build();

}
