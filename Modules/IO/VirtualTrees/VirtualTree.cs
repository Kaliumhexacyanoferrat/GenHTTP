using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO.VirtualTrees;

public sealed class VirtualTree : IResourceTree
{

    #region Initialization

    public VirtualTree(Dictionary<PathSegment, Func<IResourceContainer, IResourceNode>> nodes, Dictionary<PathSegment, IResource> resources)
    {
        var built = new Dictionary<PathSegment, IResourceNode>(nodes.Count);

        foreach (var node in nodes)
        {
            built.Add(node.Key, node.Value(this));
        }

        Nodes = built;
        Resources = resources;
    }

    #endregion

    #region Get-/Setters

    private Dictionary<PathSegment, IResourceNode> Nodes { get; }

    private Dictionary<PathSegment, IResource> Resources { get; }

    public DateTime? Modified
    {
        get
        {
            return Nodes.Select(n => n.Value.Modified)
                        .Where(n => n != null)
                        .Union
                        (
                            Resources.Select(r => r.Value.Modified)
                                     .Where(r => r != null)
                        )
                        .DefaultIfEmpty(null)
                        .Max();
        }
    }

    #endregion

    #region Functionality

    public ValueTask<IReadOnlyCollection<IResourceNode>> GetNodes() => new(Nodes.Values);

    public ValueTask<IReadOnlyCollection<IResource>> GetResources() => new(Resources.Values);

    public ValueTask<IResourceNode?> TryGetNodeAsync(PathSegment segment) => new(Nodes.GetValueOrDefault(segment));

    public ValueTask<IResource?> TryGetResourceAsync(PathSegment segment) => new(Resources.GetValueOrDefault(segment));

    #endregion

}
