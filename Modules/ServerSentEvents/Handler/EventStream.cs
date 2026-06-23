using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Conversion.Formatters;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Modules.ServerSentEvents.Handler;

public sealed class EventStream : IResponseContent
{
    private const int ErrorRetryTimeout = 30_000;

    #region Get-/Setters

    public ulong? Length => null;

    public ContentType? Type => ContentType.TextEventStream;

    public ReadOnlyMemory<byte>? Encoding => null;

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

    public async ValueTask WriteAsync(Stream target, uint bufferSize)
    {
        var connection = new EventConnection(Request, LastEventId, target, Formatters);

        try
        {
            await Generator(connection);
        }
        catch (Exception e)
        {
            var logger = Request.Server.Logging.CreateLogger(GetType());

            logger.LogError(e, "Unhandled exception in event stream generator");

            await connection.RetryAsync(ErrorRetryTimeout);
        }
    }

    public ValueTask WriteAsync(IResponseSink sink) => WriteAsync(sink.Stream, 0);

}
