using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.IO.FileSystem;

public sealed class DirectoryTreeBuilder : IBuilder<IResourceTree>
{
    private DirectoryInfo? _directory;

    #region Functionality

    public DirectoryTreeBuilder Directory(DirectoryInfo directory)
    {
        _directory = directory;
        return this;
    }

    public IResourceTree Build()
    {
        var directory = _directory ?? throw new BuilderMissingPropertyException("directory");

        if (!directory.Exists)
        {
            throw new DirectoryNotFoundException($"Directory '{directory.FullName}' does not exist");
        }

        return new DirectoryTree(directory);
    }

    #endregion

}
