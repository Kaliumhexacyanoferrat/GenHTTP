using GenHTTP.Api.Content.Caching;

namespace GenHTTP.Modules.Caching.Memory;

public sealed class MemoryCache<T> : ICache<T>
{
    private readonly Dictionary<string, Dictionary<string, T>> _cache = new();

    private readonly SemaphoreSlim _sync = new(1);

    #region Functionality

    public ValueTask<T[]> GetEntriesAsync(string key)
    {
        _sync.Wait();

        try
        {
            if (_cache.TryGetValue(key, out var entries))
            {
                return new ValueTask<T[]>(entries.Values.ToArray());
            }

            return new ValueTask<T[]>([]);
        }
        finally
        {
            _sync.Release();
        }
    }

    public ValueTask<T?> GetEntryAsync(string key, string variation)
    {
        _sync.Wait();

        try
        {
            if (_cache.TryGetValue(key, out var entries))
            {
                if (entries.TryGetValue(variation, out var value))
                {
                    return new ValueTask<T?>(value);
                }
            }

            return new ValueTask<T?>();
        }
        finally
        {
            _sync.Release();
        }
    }

    public ValueTask StoreAsync(string key, string variation, T? entry)
    {
        _sync.Wait();

        try
        {
            if (!_cache.TryGetValue(key, out var value))
            {
                value = new Dictionary<string, T>();
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

            return new ValueTask();
        }
        finally
        {
            _sync.Release();
        }
    }

    public ValueTask StoreDirectAsync(string key, string variation, Func<Stream, ValueTask> asyncWriter) => throw new NotSupportedException("Direct storage is not supported by the memory cache");

    #endregion

}
