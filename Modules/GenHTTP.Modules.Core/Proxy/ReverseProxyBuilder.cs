using System;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Core.Proxy
{

    public class ReverseProxyBuilder : IHandlerBuilder
    {
        private string? _Upstream;

        private TimeSpan _ConnectTimeout = TimeSpan.FromSeconds(10);
        private TimeSpan _ReadTimeout = TimeSpan.FromSeconds(60);

        #region Functionality

        public ReverseProxyBuilder Upstream(string upstream)
        {
            _Upstream = upstream;

            if (_Upstream.EndsWith("/"))
            {
                _Upstream = _Upstream.Substring(0, _Upstream.Length - 1);
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

        public IHandler Build(IHandler parent)
        {
            if (_Upstream == null)
            {
                throw new BuilderMissingPropertyException("Upstream");
            }

            return new ReverseProxyRouter(parent, _Upstream, _ConnectTimeout, _ReadTimeout);
        }

        #endregion

    }

}
