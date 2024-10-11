using GenHTTP.Api.Content.IO;

namespace GenHTTP.Modules.IO.FileSystem;

internal class DirectoryNode : DirectoryContainer, IResourceNode
{

    #region Get-/Setters

    public string Name => Directory.Name;

    public IResourceContainer Parent { get; }

    #endregion

    #region Initialization

    internal DirectoryNode(DirectoryInfo directory, IResourceContainer parent) : base(directory)
    {
            Parent = parent;
        }

    #endregion

}
