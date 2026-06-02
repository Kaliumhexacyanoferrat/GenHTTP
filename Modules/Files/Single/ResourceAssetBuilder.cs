using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Files.Single;

public class ResourceAssetBuilder(IResource resource) : IHandlerBuilder<ResourceAssetBuilder>
{
    private readonly List<IConcernBuilder> _concerns = [];

    private bool _asDownload;

    private string? _fileName;

    private ContentType? _contentType;

    public ResourceAssetBuilder AsDownload(string? fileName = null)
    {
        _asDownload = true;
        _fileName = fileName;
        return this;
    }

    public ResourceAssetBuilder Type(ContentType type)
    {
        _contentType = type;
        return this;
    }

    public ResourceAssetBuilder Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);
        return this;
    }

    public IHandler Build()
        => Concerns.Chain(_concerns, new ResourceAssetHandler(resource, _asDownload, _fileName, _contentType));

}
