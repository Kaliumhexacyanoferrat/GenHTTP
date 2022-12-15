using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using GenHTTP.Api.Content.IO;

namespace GenHTTP.Modules.IO.VirtualTrees
{

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

        public IAsyncEnumerable<IResourceNode> GetNodes() => Container.GetNodes().AsAsyncEnumerable();

        public IAsyncEnumerable<IResource> GetResources() => Container.GetResources().AsAsyncEnumerable();

        public ValueTask<IResourceNode?> TryGetNodeAsync(string name) => Container.TryGetNodeAsync(name);

        public ValueTask<IResource?> TryGetResourceAsync(string name) => Container.TryGetResourceAsync(name);

        #endregion

    }

}
