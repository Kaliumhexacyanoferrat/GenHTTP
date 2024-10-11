using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.IO.Providers;

namespace GenHTTP.Modules.IO;

/// <summary>
/// Provides a folder structure (provided by a resource tree) to
/// requesting clients.
/// </summary>
/// <remarks>
/// This handler can be used to serve "static" resources alongside
/// with your application.
/// </remarks>
public static class Resources
{

    /// <summary>
    /// Creates a new handler that will serve the resources provided
    /// by the given tree.
    /// </summary>
    /// <param name="tree">The resource tree to read resourced from</param>
    public static ResourceHandlerBuilder From(IBuilder<IResourceTree> tree) => From(tree.Build());

    /// <summary>
    /// Creates a new handler that will serve the resources provided
    /// by the given tree.
    /// </summary>
    /// <param name="tree">The resource tree to read resourced from</param>
    public static ResourceHandlerBuilder From(IResourceTree tree) => new ResourceHandlerBuilder().Tree(tree);

}
