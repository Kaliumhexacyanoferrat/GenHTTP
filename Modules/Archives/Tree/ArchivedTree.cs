using GenHTTP.Api.Content.IO;
using GenHTTP.Modules.IO.Streaming;
using GenHTTP.Modules.IO.Tracking;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Readers;

namespace GenHTTP.Modules.Archives.Tree;

/// <summary>
/// A tree implementation that expands the given archive into a fully
/// browsable tree instance.
/// </summary>
/// <param name="source">The resource granting access to the archive</param>
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

        if (input.CanSeek)
        {
            try
            {
                return LoadWithArchiveFactory(input);
            }
            catch
            {
                input.Position = 0;
            }
        }

        return LoadWithReaderFactory(input);
    }

    private ArchiveNode LoadWithArchiveFactory(Stream input)
    {
        using var archive = ArchiveFactory.OpenArchive(input);

        // archive factory does not automatically handle .tar.gz, reader factory does
        if (archive.Type == ArchiveType.Tar)
        {
            input.Position = 0;
            return LoadWithReaderFactory(input);
        }

        var root = new ArchiveNode(null, null);

        foreach (var entry in archive.Entries)
        {
            if (entry.IsDirectory)
            {
                AddDirectory(root, entry);
            }
            else
            {
                AddFile(root, entry, GetArchiveStream);
            }
        }

        return root;
    }

    private ArchiveNode LoadWithReaderFactory(Stream input)
    {
        using var reader = ReaderFactory.OpenReader(input);

        var root = new ArchiveNode(null, null);

        while (reader.MoveToNextEntry())
        {
            if (reader.Entry.IsDirectory)
            {
                AddDirectory(root, reader.Entry);
            }
            else
            {
                AddFile(root, reader.Entry, GetReaderStream);
            }
        }

        return root;
    }

    private static void AddDirectory(ArchiveNode root, IEntry entry)
    {
        if (entry.Key != null)
        {
            if (entry.Key == "./") return;

            var parts = GetParts(entry);

            var current = root;

            foreach (var part in parts)
            {
                current = current.GetOrCreate(part);
            }

            current.Adapt(entry);
        }
    }

    private void AddFile(ArchiveNode root, IEntry entry, Func<Stream, string, StreamWithDependency> streamFactory)
    {
        if (entry.Key != null)
        {
            var parts = GetParts(entry);

            var node = root;

            for (var i = 0; i < parts.Length - 1; i++)
            {
                node = node.GetOrCreate(parts[i]);
            }

            node.AddFile(parts.Last(), entry, source, streamFactory);
        }
    }

    private static StreamWithDependency GetArchiveStream(Stream input, string key)
    {
        var archive = ArchiveFactory.OpenArchive(input);

        var entry = archive.Entries.FirstOrDefault(e => e.Key == key);

        if (entry != null)
        {
            return new(entry.OpenEntryStream(), archive);
        }

        archive.Dispose();

        throw new InvalidOperationException($"Unable to find resource '{key}' in archive");
    }

    private static StreamWithDependency GetReaderStream(Stream input, string key)
    {
        var reader = ReaderFactory.OpenReader(input);

        while (reader.MoveToNextEntry())
        {
            if (reader.Entry.Key == key)
            {
                return new StreamWithDependency(reader.OpenEntryStream(), reader);
            }
        }

        reader.Dispose();

        throw new InvalidOperationException($"Unable to find resource '{key}' in archive");

    }

    private async ValueTask<bool> CheckUpdateNeededAsync() => _root == null || await source.CheckChangedAsync();

    private static string[] GetParts(IEntry entry)
    {
        var key = entry.Key!;

        if (key.StartsWith('.'))
        {
            key = key[1..];
        }

        return key.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries);
    }

}
