using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.ServerSentEvents.Handler;

public class EventSourceHandler : IHandler
{

    #region Get-/Setters

    public IHandler Parent { get; }

    private Func<IRequest, string?, ValueTask<bool>>? Inspector { get; }

    private Func<IEventConnection, ValueTask> Generator { get; }

    #endregion

    #region Initialization

    public EventSourceHandler(IHandler parent, Func<IRequest, string?, ValueTask<bool>>? inspector, Func<IEventConnection, ValueTask> generator)
    {
        Parent = parent;

        Inspector = inspector;
        Generator = generator;
    }

    #endregion

    #region Functionality

    public ValueTask PrepareAsync() => new();

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        if (request.Method.KnownMethod != RequestMethod.Get)
        {
            throw new ProviderException(ResponseStatus.MethodNotAllowed, "Server Sent Events require a GET request to establish a connection");
        }

        request.Headers.TryGetValue("Last-Event-ID", out var lastId);

        if ((Inspector != null) && !await Inspector(request, lastId))
        {
            return request.Respond()
                          .Status(ResponseStatus.NoContent)
                          .Build();
        }

        var content = new EventStream(request, lastId, Generator);

        return request.Respond()
                      .Type(FlexibleContentType.Get(ContentType.TextEventStream))
                      .Status(ResponseStatus.Ok)
                      .Content(content)
                      .Build();
    }

    #endregion

}
