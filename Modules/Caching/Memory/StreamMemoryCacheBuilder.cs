using System.IO;

using GenHTTP.Api.Content.Caching;

namespace GenHTTP.Modules.Caching.Memory
{

    public sealed class StreamMemoryCacheBuilder<T> : IMemoryCacheBuilder<T>
    {

        public ICache<T> Build()
        {
            return (ICache<T>)(ICache<Stream>)new StreamMemoryCache();
        }

    }

}
