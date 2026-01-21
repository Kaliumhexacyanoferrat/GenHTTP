using GenHTTP.Api.Content.IO;

using SharpCompress.Common;

namespace GenHTTP.Modules.Archives.Tree;

public sealed class ArchiveNode(string? nodeName, IResourceContainer? parent) : IResourceNode
{
    private readonly Dictionary<string, ArchiveNode> _children = new(StringComparer.OrdinalIgnoreCase);

    private readonly Dictionary<string, ArchiveResource> _resources = new(StringComparer.OrdinalIgnoreCase);

    public string Name => nodeName ?? throw new InvalidOperationException("Root node does not have a name");

    public IResourceContainer Parent => parent ?? throw new InvalidOperationException("Root node does not have a parent node");

    public DateTime? Modified { get; private set; }

    public ValueTask<IResourceNode?> TryGetNodeAsync(string name) => _children.TryGetValue(name, out var node) ? new(node) : ValueTask.FromResult<IResourceNode?>(null);

    public ValueTask<IReadOnlyCollection<IResourceNode>> GetNodes() => ValueTask.FromResult<IReadOnlyCollection<IResourceNode>>(_children.Values);

    public ValueTask<IResource?> TryGetResourceAsync(string name) => throw new NotImplementedException();

    public ValueTask<IReadOnlyCollection<IResource>> GetResources() => throw new NotImplementedException();

    public ArchiveNode GetOrCreate(string child)
    {
        if (_children.TryGetValue(child, out var found))
        {
            return found;
        }

        var created = new ArchiveNode(child, this);

        _children.Add(child, created);

        return created;
    }

    public ArchiveResource AddFile(string name, IEntry entry, IResource archive)
    {
        if (_resources.TryGetValue(name, out var found))
        {
            return found;
        }

        var created = new ArchiveResource(archive, entry, name);

        _resources.Add(name, created);

        return created;
    }

    public void Adapt(IEntry entry)
    {
        Modified = entry.LastModifiedTime;
    }

}
