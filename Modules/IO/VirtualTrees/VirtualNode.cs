using GenHTTP.Api.Content.IO;

namespace GenHTTP.Modules.IO.VirtualTrees;

public sealed class VirtualNode : IResourceNode
{

    #region Get-/Setters

    public string Name { get; }

    public IResourceContainer Parent { get; }

    public DateTime? Modified => Container.Modified;

    private IResourceContainer Container { get; }

    #endregion

    #region Initialization

    public VirtualNode(IResourceContainer parent, string name, IResourceContainer container)
    {
            Parent = parent;

            Name = name;
            Container = container;
        }

    #endregion

    #region Functionaliy

    public ValueTask<IReadOnlyCollection<IResourceNode>> GetNodes() => Container.GetNodes();

    public ValueTask<IReadOnlyCollection<IResource>> GetResources() => Container.GetResources();

    public ValueTask<IResourceNode?> TryGetNodeAsync(string name) => Container.TryGetNodeAsync(name);

    public ValueTask<IResource?> TryGetResourceAsync(string name) => Container.TryGetResourceAsync(name);

    #endregion

}
