using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using GenHTTP.Api.Content.IO;

namespace GenHTTP.Modules.IO.Embedded
{

    internal class EmbeddedResourceContainer : IResourceContainer
    {
        private readonly Dictionary<string, IResourceNode> _Nodes = new Dictionary<string, IResourceNode>();

        private readonly Dictionary<string, IResource> _Resources = new Dictionary<string, IResource>();

        #region Initialization

        protected EmbeddedResourceContainer(Assembly source, string prefix)
        {
            foreach (var resource in source.GetManifestResourceNames())
            {
                var index = resource.IndexOf(prefix);

                if (index > -1)
                {
                    var remainder = resource.Substring(index + prefix.Length + 1);

                    var parts = remainder.Split(".".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                    if (parts.Length <= 2)
                    {
                        var file = Resource.FromAssembly(source, resource)
                                           .Name(remainder)
                                           .Build();

                        _Resources.Add(remainder, file);
                    }
                    else
                    {
                        var childName = parts[0];

                        if (!_Nodes.ContainsKey(childName))
                        {
                            var childPrefix = $"{prefix}.{childName}";

                            var node = new EmbeddedResourceNode(source, childPrefix, this, childName);

                            _Nodes.Add(childName, node);
                        }
                    }
                }
            }

        }

        #endregion

        #region Functionality

        public IEnumerable<IResourceNode> GetNodes() => _Nodes.Values;

        public IEnumerable<IResource> GetResources() => _Resources.Values;

        public bool TryGetNode(string name, [MaybeNullWhen(false)] out IResourceNode node) => _Nodes.TryGetValue(name, out node);

        public bool TryGetResource(string name, [MaybeNullWhen(false)] out IResource node) => _Resources.TryGetValue(name, out node);

        #endregion

    }

}
