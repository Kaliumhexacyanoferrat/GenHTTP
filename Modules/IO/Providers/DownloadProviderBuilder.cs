using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO.Providers;

public sealed class DownloadProviderBuilder : IHandlerBuilder<DownloadProviderBuilder>
{

    private readonly List<IConcernBuilder> _concerns = [];

    private FlexibleContentType? _contentType;

    private string? _fileName;
    private IResource? _resourceProvider;

    #region Functionality

    public DownloadProviderBuilder Resource(IResource resource)
    {
        _resourceProvider = resource;
        return this;
    }

    public DownloadProviderBuilder Type(ContentType contentType, string? charset = null) => Type(FlexibleContentType.Get(contentType, charset));

    public DownloadProviderBuilder Type(FlexibleContentType contentType)
    {
        _contentType = contentType;
        return this;
    }

    public DownloadProviderBuilder FileName(string fileName)
    {
        _fileName = fileName;
        return this;
    }

    public DownloadProviderBuilder Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);
        return this;
    }

    public IHandler Build()
    {
        var resource = _resourceProvider ?? throw new BuilderMissingPropertyException("resourceProvider");

        return Concerns.Chain(_concerns,  new DownloadProvider( resource, _fileName, _contentType));
    }

    #endregion

}
