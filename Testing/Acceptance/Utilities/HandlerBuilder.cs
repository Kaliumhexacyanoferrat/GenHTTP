using GenHTTP.Api.Content;

namespace GenHTTP.Testing.Acceptance.Utilities;

public class HandlerBuilder : IHandlerBuilder<HandlerBuilder>
{
    private readonly List<IConcernBuilder> _concerns = [];

    private readonly IHandler _handler;

    public HandlerBuilder(IHandler handler) { _handler = handler; }

    public HandlerBuilder Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);
        return this;
    }

    public IHandler Build()
    {
        return Concerns.Chain(_concerns, _handler);
    }

}
