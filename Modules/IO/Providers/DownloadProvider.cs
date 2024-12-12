using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.IO.Providers;

public sealed class DownloadProvider : IHandler
{

    #region Get-/Setters

    public IResource Resource { get; }

    public string? FileName { get; }

    private FlexibleContentType ContentType { get; }

    #endregion

    #region Initialization

    public DownloadProvider(IResource resourceProvider, string? fileName, FlexibleContentType? contentType)
    {
        Resource = resourceProvider;

        FileName = fileName ?? Resource.Name;

        ContentType = contentType ?? Resource.ContentType ?? FlexibleContentType.Get(FileName?.GuessContentType() ?? Api.Protocol.ContentType.ApplicationForceDownload);
    }

    #endregion

    #region Functionality

    public ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        if (!request.Target.Ended)
        {
            return new ValueTask<IResponse?>();
        }

        if (!request.HasType(RequestMethod.Get, RequestMethod.Head))
        {
            throw new ProviderException(ResponseStatus.MethodNotAllowed, "Only GET requests are allowed by this handler", (b) => b.Header("Allow", "GET"));
        }

        var response = request.Respond()
                              .Content(Resource)
                              .Type(ContentType);

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
