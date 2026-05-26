using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.IO.Streaming;

using SharpCompress.Common;

namespace GenHTTP.Modules.Archives.Tree;

public sealed class ArchiveNode(string? nodeName, IResourceContainer? parent) : IResourceNode
{
    private readonly Dictionary<PathSegment, ArchiveNode> _children = new();

    private readonly Dictionary<PathSegment, ArchiveResource> _resources = new();

    #region Get-/Setters

    public string Name => nodeName ?? throw new InvalidOperationException("Root node does not have a name");

    public IResourceContainer Parent => parent ?? throw new InvalidOperationException("Root node does not have a parent node");

    public DateTime? Modified { get; private set; }

    #endregion

    #region Functionality

    public ValueTask<IResourceNode?> TryGetNodeAsync(PathSegment segment) => _children.TryGetValue(segment, out var node) ? new(node) : ValueTask.FromResult<IResourceNode?>(null);

    public ValueTask<IReadOnlyCollection<IResourceNode>> GetNodes() => ValueTask.FromResult<IReadOnlyCollection<IResourceNode>>(_children.Values);

    public ValueTask<IResource?> TryGetResourceAsync(PathSegment segment) => _resources.TryGetValue(segment, out var resource) ? new(resource) : ValueTask.FromResult<IResource?>(null);

    public ValueTask<IReadOnlyCollection<IResource>> GetResources() => ValueTask.FromResult<IReadOnlyCollection<IResource>>(_resources.Values);

    internal ArchiveNode GetOrCreate(string child)
    {
        if (_children.TryGetValue(new(child), out var found))
        {
            return found;
        }

        var created = new ArchiveNode(child, this);

        _children.Add(new(child), created);

        return created;
    }

    internal void AddFile(string name, IEntry entry, IResource archive, Func<Stream, string, StreamWithDependency> streamFactory)
    {
        _resources.Add(new(name), new ArchiveResource(archive, entry, name, streamFactory));
    }

    internal void Adapt(IEntry entry)
    {
        Modified = entry.LastModifiedTime;
    }

    #endregion

}
