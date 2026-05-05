using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;

namespace GenHTTP.Modules.Redirects.Provider;

public sealed class RedirectProvider : IHandler
{

    #region Get-/Setters

    public Uri Target { get; }

    private string StringTarget { get; }

    public bool Temporary { get; }

    #endregion

    #region Initialization

    public RedirectProvider(Uri location, bool temporary)
    {
        Target = location;
        StringTarget = location.ToString();

        Temporary = temporary;
    }

    #endregion

    #region Functionality

    public ValueTask PrepareAsync() => ValueTask.CompletedTask;

    public ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var resolved = ResolveRoute(request);

        var status = MapStatus(request, Temporary);

        var response = request.Respond()
                              .Header("Location", resolved)
                              .Status(status);

        return new ValueTask<IResponse?>(response.Build());
    }

    private string ResolveRoute(IRequest request)
    {
        if (Target.IsAbsoluteUri)
        {
            return StringTarget;
        }

        // todo: var protocol = request.EndPoint.Secure ? "https://" : "http://";

        return $"http://{request.Host}{StringTarget}";
    }

    private static ResponseStatus MapStatus(IRequest request, bool temporary)
    {
        if (request.HasType(RequestMethod.Get, RequestMethod.Head))
        {
            return temporary ? ResponseStatus.TemporaryRedirect : ResponseStatus.MovedPermanently;
        }

        return temporary ? ResponseStatus.SeeOther : ResponseStatus.PermanentRedirect;
    }

    #endregion

}
