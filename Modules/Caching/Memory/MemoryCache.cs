using AsyncKeyedLock;
using GenHTTP.Api.Content.Caching;
using System.Collections.Concurrent;

namespace GenHTTP.Modules.Caching.Memory;

public sealed class MemoryCache<T> : ICache<T>
{
    private readonly ConcurrentDictionary<string, Dictionary<string, T>> _cache = new();
    private readonly AsyncKeyedLocker<string> _sync = new();

    #region Functionality

    public async ValueTask<T[]> GetEntriesAsync(string key)
    {
        if (_cache.TryGetValue(key, out var entries))
        {
            return [.. entries.Values];
        }

        using var _ = await _sync.LockAsync(key).ConfigureAwait(false);

        if (_cache.TryGetValue(key, out entries))
        {
            return [.. entries.Values];
        }

        return [];
    }

    public async ValueTask<T?> GetEntryAsync(string key, string variation)
    {
        using var _ = await _sync.LockAsync(key).ConfigureAwait(false);

        if (_cache.TryGetValue(key, out var entries))
        {
            if (entries.TryGetValue(variation, out var value))
            {
                return value;
            }
        }

        return default;
    }

    public async ValueTask StoreAsync(string key, string variation, T? entry)
    {
        using var _ = await _sync.LockAsync(key).ConfigureAwait(false);

        if (!_cache.TryGetValue(key, out var value))
        {
            value = [];
            _cache[key] = value;
        }

        if (entry != null)
        {
            value[variation] = entry;
        }
        else
        {
            value.Remove(variation);
        }
    }

    public ValueTask StoreDirectAsync(string key, string variation, Func<Stream, ValueTask> asyncWriter) => throw new NotSupportedException("Direct storage is not supported by the memory cache");

    #endregion

}
