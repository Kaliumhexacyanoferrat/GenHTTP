using System;
using System.IO;
using System.Threading.Tasks;

namespace GenHTTP.Api.Content.Caching
{

    public interface ICache<T>
    {

        ValueTask<T[]> GetEntriesAsync(string key);

        ValueTask<T?> GetEntryAsync(string key, string variation);

        ValueTask StoreAsync(string key, string variation, T? entry);

        ValueTask StoreDirectAsync(string key, string variation, Func<Stream, ValueTask> asyncWriter);

    }

}
