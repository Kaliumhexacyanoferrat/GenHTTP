using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using GenHTTP.Api.Content.IO;

namespace GenHTTP.Modules.IO.VirtualTrees
{

    public sealed class VirtualTree : IResourceTree
    {

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

        #region Functionality

        public IEnumerable<IResourceNode> GetNodes() => Nodes.Values;

        public IEnumerable<IResource> GetResources() => Resources.Values;

        public bool TryGetNode(string name, [MaybeNullWhen(false)] out IResourceNode node) => Nodes.TryGetValue(name, out node);

        public bool TryGetResource(string name, [MaybeNullWhen(false)] out IResource resource) => Resources.TryGetValue(name, out resource);

        #endregion

    }

}
