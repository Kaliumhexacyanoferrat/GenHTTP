using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO.Streaming;

namespace GenHTTP.Modules.IO.Providers;

public sealed class ContentProvider : IHandler
{
    // todo
    private readonly ReadOnlyMemory<byte> _phrase = "OK"u8.ToArray();

    private readonly ReadOnlyMemory<byte> _contentTypeName = "Content-Type"u8.ToArray();
    private readonly ReadOnlyMemory<byte> _contentTypeValue = "text/plain"u8.ToArray();

    private readonly ReadOnlyMemory<byte> _contentLengthName = "Content-Length"u8.ToArray();

    #region Get-/Setters

    public IResource Resource { get; }

    private IResponseContent Content { get; }

    // private FlexibleContentType ContentType { get; }

    #endregion

    #region Initialization

    public ContentProvider(IResource resourceProvider)
    {
        Resource = resourceProvider;

        Content = new ResourceContent(Resource);

       // ContentType = Resource.ContentType ?? FlexibleContentType.Get(Resource.Name?.GuessContentType() ?? Api.Protocol.ContentType.ApplicationForceDownload);
    }

    #endregion

    #region Functionality

    public ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var response = request.Respond()
                              .Raw()
                              .Status(200, _phrase)
                              .Content(Content)
                              .Header(_contentTypeName, _contentTypeValue);

        /*if (Resource.Modified != null)
        {
            response.Modified(Resource.Modified.Value);
        }/*/

        return new(response.Build());
    }

    public ValueTask PrepareAsync() => ValueTask.CompletedTask;

    #endregion

}
