using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol.Raw;

namespace GenHTTP.Modules.IO.VirtualTrees;

public sealed class VirtualNode : IResourceNode
{

    #region Initialization

    public VirtualNode(IResourceContainer parent, string name, IResourceContainer container)
    {
        Parent = parent;

        Name = name;
        Container = container;
    }

    #endregion

    #region Get-/Setters

    public string Name { get; }

    public IResourceContainer Parent { get; }

    public DateTime? Modified => Container.Modified;

    private IResourceContainer Container { get; }

    #endregion

    #region Functionaliy

    public ValueTask<IReadOnlyCollection<IResourceNode>> GetNodes() => Container.GetNodes();

    public ValueTask<IReadOnlyCollection<IResource>> GetResources() => Container.GetResources();

    public ValueTask<IResourceNode?> TryGetNodeAsync(PathSegment segment) => Container.TryGetNodeAsync(segment);

    public ValueTask<IResource?> TryGetResourceAsync(PathSegment segment) => Container.TryGetResourceAsync(segment);

    #endregion

}
