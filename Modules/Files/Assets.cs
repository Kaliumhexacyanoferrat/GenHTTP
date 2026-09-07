using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.Files.Multi;

namespace GenHTTP.Modules.Files;

/// <summary>
/// Serves resources, either static or dynamically changing.
/// </summary>
public static class Assets
{

    /// <summary>
    /// Creates a handler to hierarchical serve the given resource tree. 
    /// </summary>
    /// <param name="tree">The tree to be served</param>
    /// <returns>The newly created handler to serve the tree</returns>
    public static TreeAssetsBuilder From(IBuilder<IResourceTree> tree) => new(tree.Build());

    /// <summary>
    /// Creates a handler to hierarchical serve the given resource tree. 
    /// </summary>
    /// <param name="tree">The tree to be served</param>
    /// <returns>The newly created handler to serve the tree</returns>
    public static TreeAssetsBuilder From(IResourceTree tree) => new(tree);

    /// <summary>
    /// Creates a handler to serve the given directory.
    /// </summary>
    /// <param name="directory">The directory to be served</param>
    /// <returns>The newly created handler</returns>
    /// <remarks>
    /// Compared to the generic tree based version, this handler provides
    /// additional optimizations to improve performance.
    /// </remarks>
    public static FileAssetsBuilder From(string directory) => new(new(directory));

    /// <summary>
    /// Creates a handler to serve the given directory.
    /// </summary>
    /// <param name="directory">The directory to be served</param>
    /// <returns>The newly created handler</returns>
    /// <remarks>
    /// Compared to the generic tree based version, this handler provides
    /// additional optimizations to improve performance.
    /// </remarks>
    public static FileAssetsBuilder From(DirectoryInfo directory) => new(directory);

}
