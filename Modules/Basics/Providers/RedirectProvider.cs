using System.Text.RegularExpressions;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Basics.Providers;

public sealed partial class RedirectProvider : IHandler
{
    private static readonly Regex ProtocolMatcher = CreateProtocolMatcher();

    #region Get-/Setters

    public string Target { get; }

    public bool Temporary { get; }

    #endregion

    #region Initialization

    public RedirectProvider(string location, bool temporary)
    {
        Target = location;
        Temporary = temporary;
    }

    [GeneratedRegex("^[a-z_-]+://", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex CreateProtocolMatcher();

    #endregion

    #region Functionality

    public ValueTask PrepareAsync() => ValueTask.CompletedTask;

    public ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var resolved = ResolveRoute(request, Target);

        var response = request.Respond()
                              .Header("Location", resolved);

        var status = MapStatus(request, Temporary);

        return new ValueTask<IResponse?>(response.Status(status).Build());
    }

    private static string ResolveRoute(IRequest request, string route)
    {
        if (ProtocolMatcher.IsMatch(route))
        {
            return route;
        }

        var protocol = request.EndPoint.Secure ? "https://" : "http://";

        return $"{protocol}{request.Host}{route}";
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
