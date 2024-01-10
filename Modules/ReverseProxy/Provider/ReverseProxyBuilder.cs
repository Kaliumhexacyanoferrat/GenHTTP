using System;
using System.Collections.Generic;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.ReverseProxy.Provider
{

    public sealed class ReverseProxyBuilder : IHandlerBuilder<ReverseProxyBuilder>
    {
        private string? _Upstream;

        private TimeSpan _ConnectTimeout = TimeSpan.FromSeconds(10);
        private TimeSpan _ReadTimeout = TimeSpan.FromSeconds(60);

        private readonly List<IConcernBuilder> _Concerns = new();

        #region Functionality

        public ReverseProxyBuilder Upstream(string upstream)
        {
            _Upstream = upstream;

            if (_Upstream.EndsWith('/'))
            {
                _Upstream = _Upstream[0..^1];
            }

            return this;
        }

        public ReverseProxyBuilder ConnectTimeout(TimeSpan connectTimeout)
        {
            _ConnectTimeout = connectTimeout;
            return this;
        }

        public ReverseProxyBuilder ReadTimeout(TimeSpan readTimeout)
        {
            _ReadTimeout = readTimeout;
            return this;
        }

        public ReverseProxyBuilder Add(IConcernBuilder concern)
        {
            _Concerns.Add(concern);
            return this;
        }

        public IHandler Build(IHandler parent)
        {
            if (_Upstream is null)
            {
                throw new BuilderMissingPropertyException("Upstream");
            }

            return Concerns.Chain(parent, _Concerns, (p) => new ReverseProxyProvider(p, _Upstream, _ConnectTimeout, _ReadTimeout));
        }

        #endregion

    }

}
