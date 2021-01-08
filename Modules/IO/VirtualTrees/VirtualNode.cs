using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

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

        public IEnumerable<IResourceNode> GetNodes() => Container.GetNodes();

        public IEnumerable<IResource> GetResources() => Container.GetResources();

        public bool TryGetNode(string name, [MaybeNullWhen(false)] out IResourceNode node) => Container.TryGetNode(name, out node);

        public bool TryGetResource(string name, [MaybeNullWhen(false)] out IResource resource) => Container.TryGetResource(name, out resource);

        #endregion

    }

}
