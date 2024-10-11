using GenHTTP.Api.Content.IO;

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

    public ValueTask<IResourceNode?> TryGetNodeAsync(string name)
    {
        var path = Path.Combine(Directory.FullName, name);

        var directory = new DirectoryInfo(path);

        if (directory.Exists)
        {
            return new ValueTask<IResourceNode?>(new DirectoryNode(directory, this));
        }

        return new ValueTask<IResourceNode?>();
    }

    public ValueTask<IResource?> TryGetResourceAsync(string name)
    {
        var path = Path.Combine(Directory.FullName, name);

        var file = new FileInfo(path);

        if (file.Exists)
        {
            return new ValueTask<IResource?>(Resource.FromFile(file).Build());
        }

        return new ValueTask<IResource?>();
    }

    #endregion

}
