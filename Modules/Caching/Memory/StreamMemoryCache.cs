using GenHTTP.Api.Content.Caching;

namespace GenHTTP.Modules.Caching.Memory;

public sealed class StreamMemoryCache : ICache<Stream>
{
    private readonly MemoryCache<byte[]> _cache = new();

    #region Functionality

    public async ValueTask<Stream[]> GetEntriesAsync(string key)
    {
        var entries = await _cache.GetEntriesAsync(key);

        var result = new List<Stream>(entries.Length);

        foreach (var entry in entries)
        {
            result.Add(new MemoryStream(entry));
        }

        return result.ToArray();
    }

    public async ValueTask<Stream?> GetEntryAsync(string key, string variation)
    {
        var entry = await _cache.GetEntryAsync(key, variation);

        return (entry != null) ? new MemoryStream(entry) : null;
    }

    public async ValueTask StoreAsync(string key, string variation, Stream? entry)
    {
        if (entry != null)
        {
            using var memoryStream = new MemoryStream();

            await entry.CopyToAsync(memoryStream);

            await _cache.StoreAsync(key, variation, memoryStream.ToArray());
        }
        else
        {
            await _cache.StoreAsync(key, variation, null);
        }
    }

    public async ValueTask StoreDirectAsync(string key, string variation, Func<Stream, ValueTask> asyncWriter)
    {
        using var memoryStream = new MemoryStream();

        await asyncWriter(memoryStream);

        await _cache.StoreAsync(key, variation, memoryStream.ToArray());
    }

    #endregion

}
