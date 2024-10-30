using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Conversion.Formatters;

namespace GenHTTP.Modules.ServerSentEvents.Handler;

public sealed class EventStream : IResponseContent
{

    #region Get-/Setters

    public ulong? Length => null;

    private IRequest Request { get; }

    private string? LastEventId { get; }

    private Func<IEventConnection, ValueTask> Generator { get; }

    private FormatterRegistry Formatters { get; }

    #endregion

    #region Initialization

    public EventStream(IRequest request, string? lastEventId, Func<IEventConnection, ValueTask> generator, FormatterRegistry formatters)
    {
        Request = request;
        LastEventId = lastEventId;
        Generator = generator;
        Formatters = formatters;
    }

    #endregion

    public ValueTask<ulong?> CalculateChecksumAsync() => new();

    public ValueTask WriteAsync(Stream target, uint bufferSize) => Generator(new EventConnection(Request, LastEventId, target, Formatters));

}
