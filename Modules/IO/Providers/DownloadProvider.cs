using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.IO.Streaming;

namespace GenHTTP.Modules.IO.Providers;

public sealed class DownloadProvider : IHandler
{

    #region Get-/Setters

    public IResource Resource { get; }

    public string? FileName { get; }

    private ContentType? ContentType { get; }

    #endregion

    #region Initialization

    public DownloadProvider(IResource resourceProvider, string? fileName, ContentType? contentType)
    {
        Resource = resourceProvider;

        FileName = fileName ?? Resource.Name;

        ContentType = contentType;
    }

    #endregion

    #region Functionality

    public ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var target = request.Header.Target;
        
        if (target.Current != null)
        {
            return default;
        }

        if (!request.HasType(RequestMethod.Get, RequestMethod.Head))
        {
            throw new ProviderException(ResponseStatus.MethodNotAllowed, "Only GET requests are allowed by this handler", b => b.Header("Allow", "GET"));
        }

        var response = request.Respond()
                              .Content(new ResourceContent(Resource, ContentType));

        var modified = Resource.Modified;

        /* todo if (modified != null)
        {
            response.Modified(modified.Value);
        }*/

        var fileName = FileName ?? Resource.Name;

        if (fileName is not null)
        {
            response.Header("Content-Disposition", $"attachment; filename=\"{fileName}\"");
        }
        else
        {
            response.Header("Content-Disposition", "attachment");
        }

        return new ValueTask<IResponse?>(response.Build());
    }

    public ValueTask PrepareAsync() => ValueTask.CompletedTask;

    #endregion

}
