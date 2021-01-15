using System.IO;

using GenHTTP.Api.Content.Caching;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.Caching;
using GenHTTP.Modules.ServerCaching.Provider;

namespace GenHTTP.Modules.ServerCaching
{

    public static class ServerCache
    {

        public static ServerCacheHandlerBuilder Create(ICache<CachedResponse> metaStore, ICache<Stream> dataStore)
            => new ServerCacheHandlerBuilder().MetaStore(metaStore).DataStore(dataStore);

        public static ServerCacheHandlerBuilder Create(IBuilder<ICache<CachedResponse>> metaStore, IBuilder<ICache<Stream>> dataStore)
            => Create(metaStore.Build(), dataStore.Build());

        public static ServerCacheHandlerBuilder Memory()
            => Create(Cache.Memory<CachedResponse>(), Cache.Memory<Stream>());

        public static ServerCacheHandlerBuilder TemporaryFiles()
            => Create(Cache.Memory<CachedResponse>(), Cache.TemporaryFiles<Stream>());

        public static ServerCacheHandlerBuilder Persistent(string directory)
            => Create(Cache.FileSystem<CachedResponse>(Path.Combine(directory, "meta")), Cache.FileSystem<Stream>(Path.Combine(directory, "data")));

        public static ServerCacheHandlerBuilder Persistent(DirectoryInfo directory)
            => Persistent(directory.FullName);

    }

}
