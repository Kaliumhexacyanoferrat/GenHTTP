using System;
using System.IO;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Caching;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.ServerCaching.Provider
{

    public class ServerCacheHandlerBuilder : IConcernBuilder
    {
        private ICache<CachedResponse>? _Meta;

        private ICache<Stream>? _Data;

        private bool _Invalidate = true;

        private Func<IResponse, bool>? _Predicate = null;

        #region Functionality

        public ServerCacheHandlerBuilder Invalidate(bool invalidate)
        {
            _Invalidate = invalidate;
            return this;
        }

        public ServerCacheHandlerBuilder Predicate(Func<IResponse, bool> predicate)
        {
            _Predicate = predicate;
            return this;
        }

        public ServerCacheHandlerBuilder MetaStore(IBuilder<ICache<CachedResponse>> cache)
        {
            _Meta = cache.Build();
            return this;
        }

        public ServerCacheHandlerBuilder MetaStore(ICache<CachedResponse> cache)
        {
            _Meta = cache;
            return this;
        }

        public ServerCacheHandlerBuilder DataStore(ICache<Stream> cache)
        {
            _Data = cache;
            return this;
        }

        public ServerCacheHandlerBuilder DataStore(IBuilder<ICache<Stream>> cache)
        {
            _Data = cache.Build();
            return this;
        }

        public IConcern Build(IHandler parent, Func<IHandler, IHandler> contentFactory)
        {
            var meta = _Meta ?? throw new BuilderMissingPropertyException("MetaStore");

            var data = _Data ?? throw new BuilderMissingPropertyException("DataStore");

            return new ServerCacheHandler(parent, contentFactory, meta, data, _Predicate, _Invalidate);
        }

        #endregion

    }

}
