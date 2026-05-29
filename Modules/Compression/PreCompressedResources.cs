using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.Compression.PreCompression;

namespace GenHTTP.Modules.Compression;

/// <summary>
/// Allows to serve static resources that already are pre-compressed by
/// an external system.
/// </summary>
/// <remarks>
/// Expects compressed files to be stored side-by-side with the non-compressed
/// files (so for example "file.js" and "file.br.js").
///
/// By default, no algorithms are configured for a newly created handler, so
/// you need to register them by yourself. As the supported algorithms are
/// controlled by the external system pre-compressing the files, this is
/// more efficient than looping through all compression algorithms supported
/// by GenHTTP.
/// </remarks>
public static class PreCompressedResources
{

    /// <summary>
    /// Creates a new handler that will look for existing, pre-compressed files
    /// based on the client preferences and serve them if suitable.
    /// </summary>
    /// <param name="tree">The tree to be served</param>
    /// <returns>The newly created handler builder</returns>
    public static PreCompressedResourceHandlerBuilder From(IBuilder<IResourceTree> tree)
        => From(tree.Build());

    /// <summary>
    /// Creates a new handler that will look for existing, pre-compressed files
    /// based on the client preferences and serve them if suitable.
    /// </summary>
    /// <param name="tree">The tree to be served</param>
    /// <returns>The newly created handler builder</returns>
    public static PreCompressedResourceHandlerBuilder From(IResourceTree tree)
        => new(tree);

}
