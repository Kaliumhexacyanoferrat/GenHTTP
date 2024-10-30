using GenHTTP.Api.Content;

namespace GenHTTP.Modules.ServerSentEvents.Handler;

public class EventSourceHandlerBuilder : IHandlerBuilder<EventSourceHandlerBuilder>
{
    private readonly List<IConcernBuilder> _Concerns = [];

    public EventSourceHandlerBuilder Add(IConcernBuilder concern)
    {
        _Concerns.Add(concern);
        return this;
    }

    public IHandler Build(IHandler parent)
    {
        return Concerns.Chain(_Concerns, (p) => new EventSourceHandler(p));
    }

}
