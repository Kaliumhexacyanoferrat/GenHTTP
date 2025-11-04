using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Modules.IO.Providers;

namespace GenHTTP.Modules.IO;

/// <summary>
/// Creates a handler that will generate a response from the content
/// of the given resource.
/// </summary>
public static class Content
{

    /// <summary>
    /// Creates a new handler that returns the content
    /// of the given resource.
    /// </summary>
    /// <param name="resource">The resource to be served</param>
    public static ContentProviderBuilder From(IBuilder<IResource> resource) => From(resource.Build());

    /// <summary>
    /// Creates a new handler that returns the content
    /// of the given resource.
    /// </summary>
    /// <param name="resource">The resource to be served</param>
    public static ContentProviderBuilder From(IResource resource) => new ContentProviderBuilder().Resource(resource);

}
