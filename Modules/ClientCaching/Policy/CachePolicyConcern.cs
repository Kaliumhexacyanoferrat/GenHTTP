using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;

namespace GenHTTP.Modules.ClientCaching.Policy;

public sealed class CachePolicyConcern : IConcern
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
            if (request.HasType(RequestMethod.Get) && response.Status.KnownStatus == ResponseStatus.Ok)
            {
                if (Predicate == null || Predicate(request, response))
                {
                    response.Expires = DateTime.UtcNow.Add(Duration);
                }
            }
        }

        return response;
    }

    public ValueTask PrepareAsync() => Content.PrepareAsync();

    #endregion

}
