using GenHTTP.Api.Content.Caching;

namespace GenHTTP.Modules.Caching.Memory;

public sealed class StreamMemoryCacheBuilder<T> : IMemoryCacheBuilder<T>
{

    public ICache<T> Build() => (ICache<T>)(ICache<Stream>)new StreamMemoryCache();
}
