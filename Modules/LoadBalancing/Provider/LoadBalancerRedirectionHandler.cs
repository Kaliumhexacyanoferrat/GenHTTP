﻿using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.LoadBalancing.Provider;

public sealed class LoadBalancerRedirectionHandler : IHandler
{

    #region Initialization

    public LoadBalancerRedirectionHandler(IHandler parent, string root)
    {
        Parent = parent;

        Root = root.EndsWith('/') ? root : $"{root}/";
    }

    #endregion

    #region Get-/Setters

    public IHandler Parent { get; }

    private string Root { get; }

    #endregion

    #region Functionality

    public ValueTask<IResponse?> HandleAsync(IRequest request) => Redirect.To(Root + request.Target.Current, true)
                                                                          .Build(this)
                                                                          .HandleAsync(request);

    public ValueTask PrepareAsync() => ValueTask.CompletedTask;

    #endregion

}
