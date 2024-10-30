using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Conversion.Formatters;

using Conv = GenHTTP.Modules.Conversion;

namespace GenHTTP.Modules.ServerSentEvents.Handler;

public sealed class EventSourceHandlerBuilder : IHandlerBuilder<EventSourceHandlerBuilder>
{
    private readonly List<IConcernBuilder> _Concerns = [];

    private FormatterBuilder _Formatters = Conv.Formatting.Default();

    private Func<IRequest, string?, ValueTask<bool>>? _Inspector;

    private Func<IEventConnection, ValueTask>? _Generator;

    #region Functionality

    public EventSourceHandlerBuilder Inspector(Func<IRequest, string?, ValueTask<bool>> inspector)
    {
        _Inspector = inspector;
        return this;
    }

    public EventSourceHandlerBuilder Generator(Func<IEventConnection, ValueTask> generator)
    {
        _Generator = generator;
        return this;
    }

    public EventSourceHandlerBuilder Formatting(FormatterBuilder formatters)
    {
        _Formatters = formatters;
        return this;
    }

    public EventSourceHandlerBuilder Add(IConcernBuilder concern)
    {
        _Concerns.Add(concern);
        return this;
    }

    public IHandler Build(IHandler parent)
    {
        var generator = _Generator ?? throw new BuilderMissingPropertyException("Generator");

        return Concerns.Chain(parent, _Concerns, (p) => new EventSourceHandler(p, _Inspector, generator, _Formatters.Build()));
    }

    #endregion

}
