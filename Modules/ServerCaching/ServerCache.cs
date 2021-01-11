using System.IO;

using GenHTTP.Api.Content.Caching;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.Caching;
using GenHTTP.Modules.ServerCaching.Provider;

namespace GenHTTP.Modules.ServerCaching
{

    /// <summary>
    /// Allows to cache generated responses so they can be served much
    /// faster when requested again.
    /// </summary>
    public static class ServerCache
    {

        /// <summary>
        /// Creates a concern that will cache generated responses in
        /// the specified caches.
        /// </summary>
        /// <param name="metaStore">The cache to be used for meta data</param>
        /// <param name="dataStore">The cache to be used for binary data</param>
        public static ServerCacheHandlerBuilder Create(ICache<CachedResponse> metaStore, ICache<Stream> dataStore)
            => new ServerCacheHandlerBuilder().MetaStore(metaStore).DataStore(dataStore);

        /// <summary>
        /// Creates a concern that will cache generated responses in
        /// the specified caches.
        /// </summary>
        /// <param name="metaStore">The cache to be used for meta data</param>
        /// <param name="dataStore">The cache to be used for binary data</param>
        public static ServerCacheHandlerBuilder Create(IBuilder<ICache<CachedResponse>> metaStore, IBuilder<ICache<Stream>> dataStore)
            => Create(metaStore.Build(), dataStore.Build());

        /// <summary>
        /// Creates a concern that will cache generated responses in memory.
        /// </summary>
        public static ServerCacheHandlerBuilder Memory()
            => Create(Cache.Memory<CachedResponse>(), Cache.Memory<Stream>());

        /// <summary>
        /// Creates a concern that will cache generated responses in a
        /// temporary folder on the file system. Uses a memory cache to
        /// store meta data.
        /// </summary>
        public static ServerCacheHandlerBuilder TemporaryFiles()
            => Create(Cache.Memory<CachedResponse>(), Cache.TemporaryFiles<Stream>());

        /// <summary>
        /// Creates a concern that will cache generated responses in the specified
        /// folder. Persists the cache accross server restarts.
        /// </summary>
        /// <param name="directory">The directory to store the cached responses in</param>
        public static ServerCacheHandlerBuilder Persistent(string directory)
            => Create(Cache.FileSystem<CachedResponse>(Path.Combine(directory, "meta")), Cache.FileSystem<Stream>(Path.Combine(directory, "data")));

        /// <summary>
        /// Creates a concern that will cache generated responses in the specified
        /// folder. Persists the cache accross server restarts.
        /// </summary>
        /// <param name="directory">The directory to store the cached responses in</param>
        public static ServerCacheHandlerBuilder Persistent(DirectoryInfo directory)
            => Persistent(directory.FullName);

    }

}
