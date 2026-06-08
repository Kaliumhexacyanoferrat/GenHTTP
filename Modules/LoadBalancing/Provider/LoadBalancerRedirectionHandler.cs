using System.Text;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Redirects;

namespace GenHTTP.Modules.LoadBalancing.Provider;

public sealed class LoadBalancerRedirectionHandler : IHandler
{

    #region Get-/Setters

    private string Root { get; }

    #endregion

    #region Initialization

    public LoadBalancerRedirectionHandler(string root)
    {
        Root = root.EndsWith('/') ? root : $"{root}/";
    }

    #endregion

    #region Functionality

    public ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var current = request.Header.Target.Current;

        var appendix = (current != null) ? Encoding.ASCII.GetString(current.Value.Bytes.Span) : string.Empty;

        return Redirect.To(Root + appendix, true)
                       .Build()
                       .HandleAsync(request);
    }

    public ValueTask PrepareAsync(IServer server) => ValueTask.CompletedTask;

    #endregion

}
