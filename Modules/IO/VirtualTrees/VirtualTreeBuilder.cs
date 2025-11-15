using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.IO.VirtualTrees;

public sealed class VirtualTreeBuilder : IBuilder<IResourceTree>
{
    private readonly Dictionary<string, Func<IResourceContainer, IResourceNode>> _nodes = new();

    private readonly Dictionary<string, IResource> _resources = new();

    #region Functionality

    /// <summary>
    /// Adds the given container with the specified name to the tree.
    /// </summary>
    /// <param name="name">The name of the node to be added</param>
    /// <param name="container">The container to be added</param>
    public VirtualTreeBuilder Add(string name, IResourceContainer container)
    {
        _nodes.Add(name, p => new VirtualNode(p, name, container));
        return this;
    }

    /// <summary>
    /// Adds the given container with the specified name to the tree.
    /// </summary>
    /// <param name="name">The name of the node to be added</param>
    /// <param name="container">The container to be added</param>
    public VirtualTreeBuilder Add(string name, IBuilder<IResourceTree> tree) => Add(name, tree.Build());

    /// <summary>
    /// Adds the given resource with the specified name to the tree.
    /// </summary>
    /// <param name="name">The name of the resource to be added</param>
    /// <param name="resource">The resource to be added</param>
    public VirtualTreeBuilder Add(string name, IResource resource)
    {
        _resources.Add(name, resource);
        return this;
    }

    /// <summary>
    /// Adds the given resource with the specified name to the tree.
    /// </summary>
    /// <param name="name">The name of the resource to be added</param>
    /// <param name="resource">The resource to be added</param>
    public VirtualTreeBuilder Add(string name, IBuilder<IResource> resource)
    {
        _resources.Add(name, resource.Build());
        return this;
    }

    public IResourceTree Build() => new VirtualTree(_nodes, _resources);

    #endregion

}
