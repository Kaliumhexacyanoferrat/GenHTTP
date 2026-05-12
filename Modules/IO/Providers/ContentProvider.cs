using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO.Streaming;

namespace GenHTTP.Modules.IO.Providers;

public sealed class ContentProvider : IHandler
{

    #region Get-/Setters

    public IResource Resource { get; }

    private IResponseContent Content { get; }

    #endregion

    #region Initialization

    public ContentProvider(IResource resource)
    {
        Resource = resource;

        var contentType = Resource.Name?.GuessContentType() ?? ContentType.ApplicationForceDownload;

        Content = new ResourceContent(Resource, contentType);
    }

    #endregion

    #region Functionality

    public ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var response = request.Respond()
                              .ToLowLevel()
                              .Status(ResponseStatus.Ok)
                              .Content(Content);

        /*if (Resource.Modified != null)
        {
            response.Modified(Resource.Modified.Value);
        }/*/

        return new(response.Build());
    }

    public ValueTask PrepareAsync() => ValueTask.CompletedTask;

    #endregion

}
