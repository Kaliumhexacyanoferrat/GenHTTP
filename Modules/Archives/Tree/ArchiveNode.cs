using GenHTTP.Api.Content.IO;

using SharpCompress.Common;

namespace GenHTTP.Modules.Archives.Tree;

public sealed class ArchiveNode(string? nodeName, IResourceContainer? parent) : IResourceNode
{
    private readonly Dictionary<string, ArchiveNode> _children = new(StringComparer.OrdinalIgnoreCase);

    private readonly Dictionary<string, ArchiveResource> _resources = new(StringComparer.OrdinalIgnoreCase);

    #region Get-/Setters

    public string Name => nodeName ?? throw new InvalidOperationException("Root node does not have a name");

    public IResourceContainer Parent => parent ?? throw new InvalidOperationException("Root node does not have a parent node");

    public DateTime? Modified { get; private set; }

    #endregion

    #region Functionality

    public ValueTask<IResourceNode?> TryGetNodeAsync(string name) => _children.TryGetValue(name, out var node) ? new(node) : ValueTask.FromResult<IResourceNode?>(null);

    public ValueTask<IReadOnlyCollection<IResourceNode>> GetNodes() => ValueTask.FromResult<IReadOnlyCollection<IResourceNode>>(_children.Values);

    public ValueTask<IResource?> TryGetResourceAsync(string name) => _resources.TryGetValue(name, out var resource) ? new(resource) : ValueTask.FromResult<IResource?>(null);

    public ValueTask<IReadOnlyCollection<IResource>> GetResources() => ValueTask.FromResult<IReadOnlyCollection<IResource>>(_resources.Values);

    internal ArchiveNode GetOrCreate(string child)
    {
        if (_children.TryGetValue(child, out var found))
        {
            return found;
        }

        var created = new ArchiveNode(child, this);

        _children.Add(child, created);

        return created;
    }

    internal void AddFile(string name, IEntry entry, IResource archive, Func<Stream, string, ValueTask<ArchiveHandle>> handleFactory)
    {
        _resources.Add(name, new ArchiveResource(archive, entry, name, handleFactory));
    }

    internal void Adapt(IEntry entry)
    {
        Modified = entry.LastModifiedTime;
    }

    #endregion

}
