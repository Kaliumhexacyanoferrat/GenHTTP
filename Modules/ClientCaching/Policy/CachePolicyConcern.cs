using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Services;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;

namespace GenHTTP.Modules.ClientCaching.Policy;

public sealed class CachePolicyConcern : IDocumentedConcern
{

    #region Get-/Setters

    public IHandler Content { get; }

    private TimeSpan Duration { get; }

    private Func<IRequest, IResponse, bool>? Predicate { get; }

    #endregion

    #region Initialization

    public CachePolicyConcern(IHandler content, TimeSpan duration, Func<IRequest, IResponse, bool>? predicate)
    {
        Content = content;

        Duration = duration;
        Predicate = predicate;
    }

    #endregion

    #region Functionality

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var response = await Content.HandleAsync(request);

        if (response != null)
        {
            if (request.HasType(RequestMethod.Get) && response.Status == ResponseStatus.Ok)
            {
                if (Predicate == null || Predicate(request, response))
                {
                    var value = DateTime.UtcNow.Add(Duration);

                    var buffer = new byte[29];
                    value.TryFormat(buffer, out _, "R");

                    response.Rebuild()
                            .Header(KnownHeaders.Expires, new(buffer));
                }
            }
        }

        return response;
    }

    public ValueTask PrepareAsync(IServer server) => Content.PrepareAsync(server);

    public void AddDocumentation(OperationDocumentation operation)
    {
        if (operation.Method == RequestMethod.Get)
        {
            operation.Add(new ResponseHeaderAttribute(ResponseStatus.Ok, "Expires", "Until when the client may cache the response"));
        }
    }

    #endregion

}
