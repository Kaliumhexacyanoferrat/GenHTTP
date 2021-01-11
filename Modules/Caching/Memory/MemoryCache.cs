using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using GenHTTP.Api.Content.Caching;

namespace GenHTTP.Modules.Caching.Memory
{

    public sealed class MemoryCache<T> : ICache<T>
    {
        private readonly Dictionary<string, Dictionary<string, T>> _Cache = new();

        #region Functionality

        public ValueTask<T[]> GetEntriesAsync(string key)
        {
            if (_Cache.TryGetValue(key, out var entries))
            {
                return new ValueTask<T[]>(entries.Values.ToArray());
            }

            return new ValueTask<T[]>(Array.Empty<T>());
        }

        public ValueTask<T?> GetEntryAsync(string key, string variation)
        {
            if (_Cache.TryGetValue(key, out var entries))
            {
                if (entries.TryGetValue(variation, out T value))
                {
                    return new ValueTask<T?>(value);
                }
            }

            return new ValueTask<T?>();
        }

        public ValueTask StoreAsync(string key, string variation, T? entry)
        {
            if (!_Cache.ContainsKey(key))
            {
                _Cache[key] = new Dictionary<string, T>();
            }

            if (entry != null)
            {
                _Cache[key][variation] = entry;
            }
            else
            {
                _Cache[key].Remove(variation);
            }

            return new ValueTask();
        }

        public ValueTask StoreDirectAsync(string key, string variations, Func<Stream, ValueTask> asyncWriter)
        {
            throw new NotSupportedException("Direct storage is not supported by the memory cache");
        }

        #endregion

    }

}
