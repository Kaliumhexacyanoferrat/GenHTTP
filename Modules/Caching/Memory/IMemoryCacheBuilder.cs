using GenHTTP.Api.Content.Caching;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.Caching.Memory;

public interface IMemoryCacheBuilder<T> : IBuilder<ICache<T>>
{
}
