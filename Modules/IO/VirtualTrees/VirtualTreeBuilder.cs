using System;
using System.Collections.Generic;

using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.IO.VirtualTrees
{

    public sealed class VirtualTreeBuilder : IBuilder<IResourceTree>
    {
        private readonly Dictionary<string, Func<IResourceContainer, IResourceNode>> _Nodes = new();

        private readonly Dictionary<string, IResource> _Resources = new();

        #region Functionality

        /// <summary>
        /// Adds the given container with the specified name to the tree.
        /// </summary>
        /// <param name="name">The name of the node to be added</param>
        /// <param name="container">The container to be added</param>
        public VirtualTreeBuilder Add(string name, IResourceContainer container)
        {
            _Nodes.Add(name, (p) => new VirtualNode(p, name, container));
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
            _Resources.Add(name, resource);
            return this;
        }

        /// <summary>
        /// Adds the given resource with the specified name to the tree.
        /// </summary>
        /// <param name="name">The name of the resource to be added</param>
        /// <param name="resource">The resource to be added</param>
        public VirtualTreeBuilder Add(string name, IBuilder<IResource> resource)
        {
            _Resources.Add(name, resource.Build());
            return this;
        }

        public IResourceTree Build() => new VirtualTree(_Nodes, _Resources);

        #endregion

    }

}
