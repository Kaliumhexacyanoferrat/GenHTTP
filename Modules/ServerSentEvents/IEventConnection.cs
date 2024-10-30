using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.ServerSentEvents;

public interface IEventConnection
{

    IRequest Request { get; }

    string? LastEventId { get; }

    bool Connected { get; }

    ValueTask<bool> CommentAsync(string comment);

    ValueTask<bool> DataAsync(string data, string? eventType = null, string? eventId = null);

    ValueTask<bool> DataAsync<T>(T data, string? eventType = null, string? eventId = null);

    ValueTask<bool> RetryAsync(int milliseconds);

}
