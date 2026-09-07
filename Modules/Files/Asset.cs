using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.Files.Single;
using GenHTTP.Modules.IO;

namespace GenHTTP.Modules.Files;

/// <summary>
/// Serves a single resource or file.
/// </summary>
public static class Asset
{

    /// <summary>
    /// Serves the given resource.
    /// </summary>
    /// <param name="resource">The resource to be served</param>
    /// <returns>The handler to serve the resource</returns>
    public static ResourceAssetBuilder From(IBuilder<IResource> resource)
        => From(resource.Build());

    /// <summary>
    /// Serves the given resource.
    /// </summary>
    /// <param name="resource">The resource to be served</param>
    /// <returns>The handler to serve the resource</returns>
    public static ResourceAssetBuilder From(IResource resource)
        => new(resource);

    /// <summary>
    /// Serves the given file.
    /// </summary>
    /// <param name="file">The file to be served</param>
    /// <returns>The handler to serve the file</returns>
    public static ResourceAssetBuilder From(FileInfo file)
        => From(Resource.FromFile(file.FullName));

    /// <summary>
    /// Serves the given file.
    /// </summary>
    /// <param name="file">The file to be served</param>
    /// <returns>The handler to serve the file</returns>
    public static ResourceAssetBuilder From(string filePath)
        => From(Resource.FromFile(filePath));

}
