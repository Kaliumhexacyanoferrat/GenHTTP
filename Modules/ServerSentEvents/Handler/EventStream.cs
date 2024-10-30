using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.ServerSentEvents.Handler;

public class EventStream : IResponseContent
{

    #region Get-/Setters

    public ulong? Length => null;

    private IRequest Request { get; }

    private string? LastEventId { get; }

    private Func<IEventConnection, ValueTask> Generator { get; }

    #endregion

    #region Initialization

    public EventStream(IRequest request, string? lastEventId, Func<IEventConnection, ValueTask> generator)
    {
        Request = request;
        LastEventId = lastEventId;
        Generator = generator;
    }

    #endregion

    public ValueTask<ulong?> CalculateChecksumAsync() => new();

    public ValueTask WriteAsync(Stream target, uint bufferSize) => Generator(new EventConnection(Request, LastEventId, target));

}
