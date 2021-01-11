using System.IO;

using GenHTTP.Api.Content.Caching;

using GenHTTP.Modules.Caching;
using GenHTTP.Modules.ServerCaching.Provider;

namespace GenHTTP.Modules.ServerCaching
{

    public static class ServerCache
    {

        public static ServerCacheHandlerBuilder Create(ICache<CachedResponse> metaStore, ICache<Stream> dataStore)
            => new ServerCacheHandlerBuilder().MetaStore(metaStore).DataStore(dataStore);


        public static ServerCacheHandlerBuilder Memory()
            => new ServerCacheHandlerBuilder().MetaStore(Cache.Memory<CachedResponse>()).DataStore(Cache.Memory<Stream>());

    }

}
