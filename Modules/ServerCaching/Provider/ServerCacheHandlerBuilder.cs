using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Caching;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.ServerCaching.Provider;

public class ServerCacheHandlerBuilder : IConcernBuilder
{

    private ICache<Stream>? _data;
    private bool _invalidate = true;
    private ICache<CachedResponse>? _meta;

    private Func<IRequest, IResponse, bool>? _predicate;

    #region Functionality

    /// <summary>
    /// If disabled, the concern will not invalidate the cache by invoking
    /// the handler to check for content changes. Therefore, it will always just
    /// cache the first response.
    /// </summary>
    /// <param name="invalidate">false, if the cache should not be invalidated automatically</param>
    public ServerCacheHandlerBuilder Invalidate(bool invalidate)
    {
        _invalidate = invalidate;
        return this;
    }

    /// <summary>
    /// A predicate that will be invoked to check, whether the given response
    /// should be cached or not.
    /// </summary>
    /// <param name="predicate">The predicate to evaluate before a response is cached</param>
    public ServerCacheHandlerBuilder Predicate(Func<IRequest, IResponse, bool> predicate)
    {
        _predicate = predicate;
        return this;
    }

    public ServerCacheHandlerBuilder MetaStore(IBuilder<ICache<CachedResponse>> cache) => MetaStore(cache.Build());

    public ServerCacheHandlerBuilder MetaStore(ICache<CachedResponse> cache)
    {
        _meta = cache;
        return this;
    }

    public ServerCacheHandlerBuilder DataStore(ICache<Stream> cache)
    {
        _data = cache;
        return this;
    }

    public ServerCacheHandlerBuilder DataStore(IBuilder<ICache<Stream>> cache) => DataStore(cache.Build());

    public IConcern Build(IHandler content)
    {
        var meta = _meta ?? throw new BuilderMissingPropertyException("MetaStore");

        var data = _data ?? throw new BuilderMissingPropertyException("DataStore");

        return new ServerCacheHandler(content, meta, data, _predicate, _invalidate);
    }

    #endregion

}
