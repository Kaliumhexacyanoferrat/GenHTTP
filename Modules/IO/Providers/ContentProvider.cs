﻿using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Basics;
using GenHTTP.Modules.IO.Streaming;

namespace GenHTTP.Modules.IO.Providers;

public sealed class ContentProvider : IHandler
{

    #region Get-/Setters

    public IResource Resource { get; }

    private IResponseContent Content { get; }

    private FlexibleContentType ContentType { get; }

    #endregion

    #region Initialization

    public ContentProvider(IResource resourceProvider)
    {
        Resource = resourceProvider;

        Content = new ResourceContent(Resource);
        ContentType = Resource.ContentType ?? FlexibleContentType.Get(Resource.Name?.GuessContentType() ?? Api.Protocol.ContentType.ApplicationForceDownload);
    }

    #endregion

    #region Functionality

    public ValueTask<IResponse?> HandleAsync(IRequest request) => request.Respond()
                                                                         .Content(Content)
                                                                         .Type(ContentType)
                                                                         .BuildTask();

    public ValueTask PrepareAsync() => ValueTask.CompletedTask;

    #endregion

}
