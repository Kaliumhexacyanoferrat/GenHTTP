using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Modules;
using GenHTTP.Api.Routing;

using GenHTTP.Modules.Core.Compression;

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

        protected ExtensionCollection _Extensions = new ExtensionCollection();

        protected Dictionary<string, ICompressionAlgorithm>? _Compression = new Dictionary<string, ICompressionAlgorithm>(StringComparer.InvariantCultureIgnoreCase);
        
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

        public IServerBuilder Console()
        {
            _Companion = new ConsoleCompanion();
            return this;
        }

        public IServerBuilder Extension(IBuilder<IServerExtension> extension)
        {
            return Extension(extension.Build());
        }

        public IServerBuilder Extension(IServerExtension extension)
        {
            _Extensions.Add(extension);
            return this;
        }

        public IServerBuilder Compression(IBuilder<ICompressionAlgorithm> algorithm)
        {
            return Compression(algorithm.Build());
        }

        public IServerBuilder Compression(ICompressionAlgorithm algorithm)
        {
            if (_Compression == null)
            {
                _Compression = new Dictionary<string, ICompressionAlgorithm>();
            }

            _Compression[algorithm.Name] = algorithm;

            return this;
        }

        public IServerBuilder Compression(bool enabled)
        {
            if (enabled)
            {
                _Compression = new Dictionary<string, ICompressionAlgorithm>();
            }
            else
            {
                _Compression = null;
            }

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
            
            if (_Compression != null)
            {
                if (!_Compression.ContainsKey("gzip"))
                {
                    _Compression.Add("gzip", new GzipAlgorithm());
                }

                _Extensions.Add(new CompressionExtension(_Compression));
            }

            return new ThreadedServer(_Companion, config, _Extensions, _Router);
        }
        
        #endregion

    }

}
