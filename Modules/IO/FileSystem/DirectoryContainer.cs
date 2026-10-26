using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO.FileSystem;

internal class DirectoryContainer : IResourceContainer
{

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
        var path = ResolveChild(segment);

        if (path != null)
        {
            var directory = new DirectoryInfo(path);

            if (directory.Exists)
            {
                return new ValueTask<IResourceNode?>(new DirectoryNode(directory, this));
            }
        }

        return new ValueTask<IResourceNode?>();
    }

    public ValueTask<IResource?> TryGetResourceAsync(PathSegment segment)
    {
        var path = ResolveChild(segment);

        if (path != null)
        {
            var file = new FileInfo(path);

            if (file.Exists)
            {
                return new(Resource.FromFile(file).Build());
            }
        }

        return default;
    }

    private string? ResolveChild(PathSegment segment)
    {
        var root = Directory.FullName.AsSpan();

        var prefixLength = Path.EndsInDirectorySeparator(root) ? root.Length : root.Length + 1;

        var length = prefixLength + segment.Bytes.Length;

        var buffer = length <= 1024 ? stackalloc char[length] : new char[length];

        root.CopyTo(buffer);

        buffer[prefixLength - 1] = Path.DirectorySeparatorChar;

        var written = prefixLength + segment.Decode(buffer[prefixLength..]);

        var path = Path.GetFullPath(new string(buffer[..written]));

        var isDirectChild = path.Length > prefixLength
                            && path.AsSpan().StartsWith(buffer[..prefixLength], StringComparison.Ordinal)
                            && path.IndexOf(Path.DirectorySeparatorChar, prefixLength) < 0;

        return isDirectChild ? path : null;
    }

    #endregion

}
