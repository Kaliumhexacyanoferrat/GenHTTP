﻿using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Pages;

namespace GenHTTP.Modules.DirectoryBrowsing.Provider;

public sealed class ListingProvider : IHandler
{

    #region Get-/Setters

    public IResourceContainer Container { get; }

    #endregion

    #region Initialization

    public ListingProvider(IResourceContainer container)
    {
        Container = container;
    }

    #endregion

    #region Functionality

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var model = new ListingModel(Container, !request.Target.Ended);

        var content = await ListingRenderer.RenderAsync(model);

        var page = await Renderer.Server.RenderAsync($"Index of {request.Target.Path}", content);

        return request.GetPage(page).Build();
    }

    public ValueTask PrepareAsync() => ValueTask.CompletedTask;

    #endregion

}
