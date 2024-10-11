using System.IO;

using GenHTTP.Api.Content.IO;

namespace GenHTTP.Modules.IO.FileSystem;

internal class DirectoryTree : DirectoryContainer, IResourceTree
{

    #region Initialization

    internal DirectoryTree(DirectoryInfo directory) : base(directory) { }

    #endregion

}
