using GenHTTP.Api.Content.IO;

using GenHTTP.Modules.IO.Tracking;

using SharpCompress.Common;
using SharpCompress.Readers;

namespace GenHTTP.Modules.Archives.Tree;

public sealed class ArchivedTree(ChangeTrackingResource source) : IResourceTree
{
    private readonly SemaphoreSlim _updateLock = new(1, 1);

    private ArchiveNode? _root;

    public DateTime? Modified => source.Modified;

    public async ValueTask<IResourceNode?> TryGetNodeAsync(string name)
    {
        await EnsureRoot();

        return await _root!.TryGetNodeAsync(name);
    }

    public async ValueTask<IReadOnlyCollection<IResourceNode>> GetNodes()
    {
        await EnsureRoot();

        return await _root!.GetNodes();
    }

    public async ValueTask<IResource?> TryGetResourceAsync(string name)
    {
        await EnsureRoot();

        return await _root!.TryGetResourceAsync(name);
    }

    public async ValueTask<IReadOnlyCollection<IResource>> GetResources()
    {
        await EnsureRoot();

        return await _root!.GetResources();
    }

    private async ValueTask<ArchiveNode> EnsureRoot()
    {
        if (await CheckUpdateNeededAsync())
        {
            await _updateLock.WaitAsync();

            try
            {
                if (await CheckUpdateNeededAsync())
                {
                    _root = await LoadArchive();
                }
            }
            finally
            {
                _updateLock.Release();
            }
        }

        return _root!;
    }

    private async ValueTask<ArchiveNode> LoadArchive()
    {
        await using var input = await source.GetContentAsync();

        using var reader = ReaderFactory.Open(input);

        var root = new ArchiveNode(null, null);

        while (await reader.MoveToNextEntryAsync())
        {
            if (reader.Entry.IsDirectory)
            {
                AddDirectory(root, reader.Entry);
            }
            else
            {
                AddFile(root, reader.Entry);
            }
        }

        return root;
    }

    private static void AddDirectory(ArchiveNode root, IEntry entry)
    {
        if (entry.Key != null)
        {
            var parts = GetParts(entry);

            var current = root;

            foreach (var part in parts)
            {
                current = current.GetOrCreate(part);
            }

            current.Adapt(entry);
        }
    }

    private void AddFile(ArchiveNode root, IEntry entry)
    {
        if (entry.Key != null)
        {
            var parts = GetParts(entry);

            var node = root;

            for (var i = 0; i < parts.Length - 1; i++)
            {
                node = node.GetOrCreate(parts[i]);
            }

            node.AddFile(parts.Last(), entry, source);
        }
    }

    private async ValueTask<bool> CheckUpdateNeededAsync() => _root == null || await source.CheckChangedAsync();

    private static string[] GetParts(IEntry entry) => entry.Key!.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries);

}
