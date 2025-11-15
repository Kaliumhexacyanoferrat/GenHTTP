using System.Reflection;
using GenHTTP.Api.Content.IO;

namespace GenHTTP.Modules.IO.Embedded;

internal class EmbeddedResourceContainer : IResourceContainer
{
    private readonly Dictionary<string, IResourceNode> _nodes = new();

    private readonly Dictionary<string, IResource> _resources = new();

    #region Initialization

    protected EmbeddedResourceContainer(Assembly source, string prefix)
    {
        Modified = source.GetModificationDate();

        foreach (var resource in source.GetManifestResourceNames())
        {
            var index = resource.IndexOf(prefix, StringComparison.Ordinal);

            if (index > -1)
            {
                var remainder = resource[(index + prefix.Length + 1)..];

                var parts = remainder.Split(".".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length <= 2)
                {
                    var file = Resource.FromAssembly(source, resource)
                                       .Name(remainder)
                                       .Build();

                    _resources.Add(remainder, file);
                }
                else
                {
                    var childName = parts[0];

                    if (!_nodes.ContainsKey(childName))
                    {
                        var childPrefix = $"{prefix}.{childName}";

                        var node = new EmbeddedResourceNode(source, childPrefix, this, childName);

                        _nodes.Add(childName, node);
                    }
                }
            }
        }

    }

    #endregion

    #region Get-/Setters

    public DateTime? Modified { get; }

    #endregion

    #region Functionality

    public ValueTask<IReadOnlyCollection<IResourceNode>> GetNodes() => new(_nodes.Values);

    public ValueTask<IReadOnlyCollection<IResource>> GetResources() => new(_resources.Values);

    public ValueTask<IResourceNode?> TryGetNodeAsync(string name) => new(_nodes.GetValueOrDefault(name));

    public ValueTask<IResource?> TryGetResourceAsync(string name) => new(_resources.GetValueOrDefault(name));

    #endregion

}
