using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;

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

    public ValueTask<IResponse?> HandleAsync(IRequest request) => Redirect.To(Root + request.Target.Current, true)
                                                                          .Build()
                                                                          .HandleAsync(request);

    public ValueTask PrepareAsync() => ValueTask.CompletedTask;

    #endregion

}
