﻿using GenHTTP.Api.Content.IO;

namespace GenHTTP.Modules.IO.VirtualTrees;

public sealed class VirtualTree : IResourceTree
{

    #region Initialization

    public VirtualTree(Dictionary<string, Func<IResourceContainer, IResourceNode>> nodes, Dictionary<string, IResource> resources)
    {
        var built = new Dictionary<string, IResourceNode>(nodes.Count);

        foreach (var node in nodes)
        {
            built.Add(node.Key, node.Value(this));
        }

        Nodes = built;
        Resources = resources;
    }

    #endregion

    #region Get-/Setters

    private Dictionary<string, IResourceNode> Nodes { get; }

    private Dictionary<string, IResource> Resources { get; }

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

    public ValueTask<IResourceNode?> TryGetNodeAsync(string name) => new(Nodes.GetValueOrDefault(name));

    public ValueTask<IResource?> TryGetResourceAsync(string name) => new(Resources.GetValueOrDefault(name));

    #endregion

}
