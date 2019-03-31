using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Modules;
using GenHTTP.Api.Routing;

namespace GenHTTP.Core.Infrastructure
{

    internal class ThreadedServerBuilder : IServerBuilder
    {
        protected ushort _Backlog = 20;
        protected ushort _Port = 8080;

        protected uint _RequestMemoryLimit = 1 * 1024 + 1024; // 1 MB
        protected uint _TransferBufferSize = 65 * 1024; // 65 KB

        protected TimeSpan _RequestReadTimeout = TimeSpan.FromSeconds(10);

        protected IRouter? _Router;
        protected IServerCompanion? _Companion;
        
        #region Functionality

        public IServerBuilder Router(IRouterBuilder routerBuilder)
        {
            return Router(routerBuilder.Build());
        }

        public IServerBuilder Router(IRouter router)
        {
            _Router = router;
            return this;
        }

        public IServerBuilder Companion(IServerCompanion companion)
        {
            _Companion = companion;
            return this;
        }

        public IServerBuilder Port(ushort port)
        {
            if (port == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(port));
            }

            _Port = port;
            return this;
        }

        public IServerBuilder Backlog(ushort backlog)
        {
            _Backlog = backlog;
            return this;
        }

        public IServerBuilder RequestReadTimeout(TimeSpan timeout)
        {
            _RequestReadTimeout = timeout;
            return this;
        }

        public IServerBuilder RequestMemoryLimit(uint limit)
        {
            _RequestMemoryLimit = limit;
            return this;
        }

        public IServerBuilder TransferBufferSize(uint bufferSize)
        {
            _TransferBufferSize = bufferSize;
            return this;
        }
        
        public IServer Build()
        {
            if (_Router == null)
            {
                throw new BuilderMissingPropertyException("Router");
            }

            var network = new NetworkConfiguration(_RequestReadTimeout, _RequestMemoryLimit, _TransferBufferSize);

            var config = new ServerConfiguration(_Port, _Backlog, network);
            
            return new ThreadedServer(_Router, _Companion, config);
        }
        
        #endregion

    }

}
