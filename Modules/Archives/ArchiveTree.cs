using GenHTTP.Api.Content.IO;

using GenHTTP.Modules.Archives.Tree;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.IO.Tracking;

namespace GenHTTP.Modules.Archives;

/// <summary>
/// Provides archives (such as RAR, ZIP or TAR files) as resource trees.
/// </summary>
/// <remarks>
/// Uses sharpcompress to open and evaluate archives. Archive parsing is
/// done once on first request. Uses change tracking to determine changes
/// to the underlying archive and reparses again if needed. Files are always
/// read from a fresh stream directly from the archive.
/// </remarks>
public static class ArchiveTree
{

    /// <summary>
    /// Create a resource tree from the given archive resource.
    /// </summary>
    /// <param name="source">The resource granting access to the archive</param>
    public static ArchivedTreeBuilder From<T>(IResourceBuilder<T> source) where T : IResourceBuilder<T> => From(source.BuildWithTracking());

    /// <summary>
    /// Create a resource tree from the given archive resource.
    /// </summary>
    /// <param name="source">The resource granting access to the archive</param>
    public static ArchivedTreeBuilder From(IResource source) => From(new(source));

    /// <summary>
    /// Create a resource tree from the given archive resource.
    /// </summary>
    /// <param name="source">The resource granting access to the archive</param>
    public static ArchivedTreeBuilder From(ChangeTrackingResource source) => new(source);

}
