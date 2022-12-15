using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        public IAsyncEnumerable<IResourceNode> GetNodes() => Nodes.Values.ToAsyncEnumerable();

        public IAsyncEnumerable<IResource> GetResources() => Resources.Values.ToAsyncEnumerable();

        public ValueTask<IResourceNode?> TryGetNodeAsync(string name) => new(Nodes.GetValueOrDefault(name));

        public ValueTask<IResource?> TryGetResourceAsync(string name) => new(Resources.GetValueOrDefault(name));

        #endregion

    }

}
