using System.Reflection;
using GenHTTP.Api.Content.IO;

namespace GenHTTP.Modules.IO.Embedded;

internal class EmbeddedResourceNode : EmbeddedResourceContainer, IResourceNode
{

    #region Initialization

    internal EmbeddedResourceNode(Assembly source, string prefix, IResourceContainer parent, string name) : base(source, prefix)
    {
        Name = name;
        Parent = parent;
    }

    #endregion

    #region Get-/Setters

    public string Name { get; }

    public IResourceContainer Parent { get; }

    #endregion

}
