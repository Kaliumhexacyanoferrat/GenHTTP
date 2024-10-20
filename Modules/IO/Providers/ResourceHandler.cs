﻿using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.IO.Providers;

public sealed class ResourceHandler : IHandler
{

    #region Initialization

    public ResourceHandler(IHandler parent, IResourceTree tree)
    {
        Parent = parent;
        Tree = tree;
    }

    #endregion

    #region Get-/Setters

    public IHandler Parent { get; }

    private IResourceTree Tree { get; }

    #endregion

    #region Functionality

    public ValueTask PrepareAsync() => ValueTask.CompletedTask;

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var (_, resource) = await Tree.Find(request.Target);

        if (resource is not null)
        {
            var type = resource.ContentType ?? FlexibleContentType.Get(resource.Name?.GuessContentType() ?? ContentType.ApplicationForceDownload);

            return request.Respond()
                          .Content(resource)
                          .Type(type)
                          .Build();
        }

        return null;
    }

    #endregion

}
