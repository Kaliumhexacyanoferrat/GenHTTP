using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Modules.IO.Providers;

namespace GenHTTP.Modules.IO;

/// <summary>
/// Generates a file download response for a given resource.
/// </summary>
public static class Download
{

    /// <summary>
    /// Creates a new download handler for the given resource.
    /// </summary>
    /// <param name="resource">The resource to be provided</param>
    public static DownloadProviderBuilder From(IBuilder<IResource> resource) => From(resource.Build());

    /// <summary>
    /// Creates a new download handler for the given resource.
    /// </summary>
    /// <param name="resource">The resource to be provided</param>
    public static DownloadProviderBuilder From(IResource resource) => new DownloadProviderBuilder().Resource(resource);
}
