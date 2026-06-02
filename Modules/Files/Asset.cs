using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Modules.Files.Single;
using GenHTTP.Modules.IO;

namespace GenHTTP.Modules.Files;

public static class Asset
{

    public static ResourceAssetBuilder From(IBuilder<IResource> resource)
        => new(resource.Build());

    public static ResourceAssetBuilder From(IResource resource)
        => new(resource);

    public static ResourceAssetBuilder From(FileInfo file)
        => From(Resource.FromFile(file.FullName));

    public static ResourceAssetBuilder From(string filePath)
        => From(Resource.FromFile(filePath));

}
