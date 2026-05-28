using System.Collections.Concurrent;

using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO.FileSystem;

internal class DirectoryContainer : IResourceContainer
{
    private readonly ConcurrentDictionary<PathSegment, IResource?> _resourceCache = new();

    #region Initialization

    protected DirectoryContainer(DirectoryInfo directory)
    {
        Directory = directory;
    }

    #endregion

    #region Get-/Setters

    protected DirectoryInfo Directory { get; }

    public DateTime? Modified => Directory.LastWriteTimeUtc;

    #endregion

    #region Functionality

    public ValueTask<IReadOnlyCollection<IResourceNode>> GetNodes()
    {
        var result = new List<IResourceNode>();

        foreach (var directory in Directory.EnumerateDirectories())
        {
            result.Add(new DirectoryNode(directory, this));
        }

        return new ValueTask<IReadOnlyCollection<IResourceNode>>(result);
    }

    public ValueTask<IReadOnlyCollection<IResource>> GetResources()
    {
        var result = new List<IResource>();

        foreach (var file in Directory.EnumerateFiles())
        {
            result.Add(Resource.FromFile(file).Build());
        }

        return new ValueTask<IReadOnlyCollection<IResource>>(result);
    }

    public ValueTask<IResourceNode?> TryGetNodeAsync(PathSegment segment)
    {
        var path = Path.Combine(Directory.FullName, segment.Decode());

        var directory = new DirectoryInfo(path);

        if (directory.Exists)
        {
            return new ValueTask<IResourceNode?>(new DirectoryNode(directory, this));
        }

        return new ValueTask<IResourceNode?>();
    }

    public ValueTask<IResource?> TryGetResourceAsync(PathSegment segment)
    {
        return new ValueTask<IResource?>(_resourceCache.GetOrAdd(segment, static (key, dir) =>
        {
            var path = Path.Combine(dir.FullName, key.Decode());
            var file = new FileInfo(path);
            return file.Exists ? Resource.FromFile(file).Build() : null;
        }, Directory));
    }

    #endregion

}
