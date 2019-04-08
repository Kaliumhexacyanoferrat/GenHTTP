using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Modules;

namespace GenHTTP.Modules.Core.Proxy
{

    public class ReverseProxyProviderBuilder : IContentBuilder
    {
        private string? _Upstream;

        private TimeSpan _ConnectTimeout = TimeSpan.FromSeconds(10);
        private TimeSpan _ReadTimeout = TimeSpan.FromSeconds(60);

        #region Functionality

        public ReverseProxyProviderBuilder Upstream(string upstream)
        {
            _Upstream = upstream;

            if (_Upstream.EndsWith("/"))
            {
                _Upstream = _Upstream.Substring(0, _Upstream.Length - 1);
            }

            return this;
        }

        public ReverseProxyProviderBuilder ConnectTimeout(TimeSpan connectTimeout)
        {
            _ConnectTimeout = connectTimeout;
            return this;
        }

        public ReverseProxyProviderBuilder ReadTimeout(TimeSpan readTimeout)
        {
            _ReadTimeout = readTimeout;
            return this;
        }

        public IContentProvider Build()
        {
            if (_Upstream == null)
            {
                throw new BuilderMissingPropertyException("Upstream");
            }

            return new ReverseProxyProvider(_Upstream, _ConnectTimeout, _ReadTimeout);
        }

        #endregion

    }

}
