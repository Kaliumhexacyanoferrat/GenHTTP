namespace GenHTTP.Api.Content.Caching;

/// <summary>
/// Saves intermediate results for fast access.
/// </summary>
/// <typeparam name="T">The type of the results to be cached</typeparam>
public interface ICache<T>
{

    /// <summary>
    /// Fetches all entries that are stored in the
    /// cache with the given key.
    /// </summary>
    /// <param name="key">The key to look up</param>
    /// <returns>The entries stored for this key</returns>
    ValueTask<T[]> GetEntriesAsync(string key);

    /// <summary>
    /// Attempts to fetch a single entry with the given key
    /// and variation.
    /// </summary>
    /// <param name="key">The key of the entry to be fetched</param>
    /// <param name="variation">The variation to be fetched</param>
    /// <returns>The requested entry, if any</returns>
    ValueTask<T?> GetEntryAsync(string key, string variation);

    /// <summary>
    /// Stores the given entry in the cache.
    /// </summary>
    /// <param name="key">The key of the entry (should be file system compatible)</param>
    /// <param name="variation">The variation specification of the entry</param>
    /// <param name="entry">The entry to be stored (or to be deleted, if null)</param>
    ValueTask StoreAsync(string key, string variation, T? entry);

    /// <summary>
    /// Exposes the stream which can be written to to
    /// update the specified entry.
    /// </summary>
    /// <param name="key">The key of the entry to be written (should be file system compatible)</param>
    /// <param name="variation">The variation specification of the entry</param>
    /// <param name="asyncWriter">A callback that allows to write the entry to the target stream</param>
    ValueTask StoreDirectAsync(string key, string variation, Func<Stream, ValueTask> asyncWriter);

}
