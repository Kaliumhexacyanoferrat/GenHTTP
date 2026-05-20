using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Conversion.Formatters;

namespace GenHTTP.Modules.ServerSentEvents.Handler;

public sealed class EventSourceHandler : IHandler
{

    #region Get-/Setters

    private Func<IRequest, string?, ValueTask<bool>>? Inspector { get; }

    private Func<IEventConnection, ValueTask> Generator { get; }

    private FormatterRegistry Formatters { get; }

    #endregion

    #region Initialization

    public EventSourceHandler(Func<IRequest, string?, ValueTask<bool>>? inspector, Func<IEventConnection, ValueTask> generator, FormatterRegistry formatters)
    {
        Inspector = inspector;
        Generator = generator;
        Formatters = formatters;
    }

    #endregion

    #region Functionality

    public ValueTask PrepareAsync() => default;

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        if (request.Header.Method != RequestMethod.Get)
        {
            throw new ProviderException(ResponseStatus.MethodNotAllowed, "Server Sent Events require a GET request to establish a connection", b => b.Header("Allow", "GET"));
        }

        var lastId = request.Header.Headers.GetEntry("Last-Event-ID");

        if ((Inspector != null) && !await Inspector(request, lastId))
        {
            return request.Respond()
                          .Status(ResponseStatus.NoContent)
                          .Build();
        }

        var content = new EventStream(request, lastId, Generator, Formatters);

        return request.Respond()
                      .Status(ResponseStatus.Ok)
                      .Content(content)
                      .Build();
    }

    #endregion

}
