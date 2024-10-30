using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.ServerSentEvents.Handler;

public class EventConnection : IEventConnection
{

    #region Get-/Setters

    public IRequest Request { get; }

    public string? LastEventId { get; }

    public bool Connected { get; }

    private Stream Target { get; }

    #endregion

    #region Initialization

    public EventConnection(IRequest request, string? lastEventId, Stream target)
    {
        Request = request;
        LastEventId = lastEventId;
        Target = target;
    }

    #endregion

    #region Functionality

    public ValueTask<bool> CommentAsync(string comment) => throw new NotImplementedException();

    public ValueTask<bool> DataAsync(string data, string? eventType = null, string? eventId = null) => throw new NotImplementedException();

    public ValueTask<bool> DataAsync<T>(T data, string? eventType = null, string? eventId = null) => throw new NotImplementedException();

    public ValueTask<bool> Retry(int milliseconds) => throw new NotImplementedException();

    #endregion

}
