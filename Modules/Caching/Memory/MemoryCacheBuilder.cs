using GenHTTP.Api.Content.Caching;

namespace GenHTTP.Modules.Caching.Memory;

public sealed class MemoryCacheBuilder<T> : IMemoryCacheBuilder<T>
{

    public ICache<T> Build() => new MemoryCache<T>();
}
