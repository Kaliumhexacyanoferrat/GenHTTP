using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using GenHTTP.Api.Content.IO;

namespace GenHTTP.Modules.IO.Embedded
{

    internal class EmbeddedResourceContainer : IResourceContainer
    {
        private readonly Dictionary<string, IResourceNode> _Nodes = new();

        private readonly Dictionary<string, IResource> _Resources = new();

        #region Get-/Setters

        public DateTime? Modified { get; }

        #endregion

        #region Initialization

        protected EmbeddedResourceContainer(Assembly source, string prefix)
        {
            Modified = source.GetModificationDate();

            foreach (var resource in source.GetManifestResourceNames())
            {
                var index = resource.IndexOf(prefix);

                if (index > -1)
                {
                    var remainder = resource[(index + prefix.Length + 1)..];

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

        public IAsyncEnumerable<IResourceNode> GetNodes() => _Nodes.Values.ToAsyncEnumerable();

        public IAsyncEnumerable<IResource> GetResources() => _Resources.Values.ToAsyncEnumerable();

        public ValueTask<IResourceNode?> TryGetNodeAsync(string name) => new(_Nodes.GetValueOrDefault(name));

        public ValueTask<IResource?> TryGetResourceAsync(string name) => new(_Resources.GetValueOrDefault(name));

        #endregion

    }

}
