using System;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Linq;
using System.Collections.Generic;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Routing;

using GenHTTP.Core.Infrastructure.Endpoints;
using GenHTTP.Core.Infrastructure.Configuration;

using GenHTTP.Modules.Core.Compression;
using GenHTTP.Modules.Core.Security;

namespace GenHTTP.Core.Infrastructure
{

    internal class ThreadedServerBuilder : IServerBuilder
    {
        private ushort _Backlog = 20;
        private ushort _Port = 8080;

        private uint _RequestMemoryLimit = 1 * 1024 + 1024; // 1 MB
        private uint _TransferBufferSize = 65 * 1024; // 65 KB

        private bool _Development = false;

        private TimeSpan _RequestReadTimeout = TimeSpan.FromSeconds(10);

        private SecureUpgrade _SecureUpgrade = Api.Infrastructure.SecureUpgrade.Force;

        private bool _StrictTransport = true;
        private StrictTransportPolicy _StrictTransportPolicy = new StrictTransportPolicy(TimeSpan.FromDays(365), true, true);

        private IRouter? _Router;
        private IServerCompanion? _Companion;

        private readonly List<IServerExtension> _Extensions = new List<IServerExtension>();

        private Dictionary<string, ICompressionAlgorithm>? _Compression = new Dictionary<string, ICompressionAlgorithm>(StringComparer.InvariantCultureIgnoreCase);

        private readonly List<EndPointConfiguration> _EndPoints = new List<EndPointConfiguration>();

        #region Content

        public IServerBuilder Router(IRouterBuilder routerBuilder)
        {
            return Router(routerBuilder.Build());
        }

        public IServerBuilder Router(IRouter router)
        {
            _Router = router;
            return this;
        }

        #endregion

        #region Infrastructure

        public IServerBuilder Console()
        {
            _Companion = new ConsoleCompanion();
            return this;
        }

        public IServerBuilder Companion(IServerCompanion companion)
        {
            _Companion = companion;
            return this;
        }

        public IServerBuilder Extension(IServerExtensionBuilder extension)
        {
            return Extension(extension.Build());
        }

        public IServerBuilder Extension(IServerExtension extension)
        {
            _Extensions.Add(extension);
            return this;
        }

        public IServerBuilder Development(bool developmentMode = true)
        {
            _Development = developmentMode;
            return this;
        }

        #endregion

        #region Binding

        public IServerBuilder Port(ushort port)
        {
            if (port == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(port));
            }

            _Port = port;
            return this;
        }

        public IServerBuilder Bind(IPAddress address, ushort port)
        {
            _EndPoints.Add(new EndPointConfiguration(address, port, null));
            return this;
        }

        public IServerBuilder Bind(IPAddress address, ushort port, string host, X509Certificate2 certificate)
        {
            return Bind(address, port, new SimpleCertificateProvider(host, certificate));
        }

        public IServerBuilder Bind(IPAddress address, ushort port, string host, X509Certificate2 certificate, SslProtocols protocols)
        {
            return Bind(address, port, new SimpleCertificateProvider(host, certificate), protocols);
        }

        public IServerBuilder Bind(IPAddress address, ushort port, ICertificateProvider certificateProvider)
        {
            _EndPoints.Add(new EndPointConfiguration(address, port, new SecurityConfiguration(certificateProvider, SslProtocols.Tls12)));
            return this;
        }

        public IServerBuilder Bind(IPAddress address, ushort port, ICertificateProvider certificateProvider, SslProtocols protocols)
        {
            _EndPoints.Add(new EndPointConfiguration(address, port, new SecurityConfiguration(certificateProvider, protocols)));
            return this;
        }

        #endregion

        #region Network settings

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

        #endregion

        #region Compression

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

        #endregion

        #region Security

        public IServerBuilder SecureUpgrade(SecureUpgrade upgradeMode)
        {
            _SecureUpgrade = upgradeMode;
            return this;
        }

        public IServerBuilder StrictTransport(TimeSpan maximumAge, bool includeSubdomains = true, bool preload = true)
        {
            _StrictTransport = true;
            _StrictTransportPolicy = new StrictTransportPolicy(maximumAge, includeSubdomains, preload);
            return this;
        }

        public IServerBuilder StrictTransport(bool enabled)
        {
            _StrictTransport = enabled;
            return this;
        }

        #endregion

        #region Builder

        public IServer Build()
        {
            if (_Router == null)
            {
                throw new BuilderMissingPropertyException("Router");
            }

            var network = new NetworkConfiguration(_RequestReadTimeout, _RequestMemoryLimit, _TransferBufferSize, _Backlog);

            var endpoints = new List<EndPointConfiguration>(_EndPoints);

            if (!endpoints.Any())
            {
                endpoints.Add(new EndPointConfiguration(IPAddress.Any, _Port, null));
                endpoints.Add(new EndPointConfiguration(IPAddress.IPv6Any, _Port, null));
            }

            var config = new ServerConfiguration(_Development, endpoints, network);

            var extensions = new ExtensionCollection();
            extensions.AddRange(_Extensions);

            if (_Compression != null)
            {
                var algorithms = new Dictionary<string, ICompressionAlgorithm>(_Compression);

                if (!algorithms.ContainsKey("gzip"))
                {
                    algorithms.Add("gzip", new GzipAlgorithm());
                }

                extensions.Add(new CompressionExtension(algorithms));
            }

            if (endpoints.Any(e => e.Security != null))
            {
                if (_SecureUpgrade != Api.Infrastructure.SecureUpgrade.None)
                {
                    extensions.Add(new SecureUpgradeExtension(_SecureUpgrade));
                }

                if (_StrictTransport)
                {
                    extensions.Add(new StrictTransportExtension(_StrictTransportPolicy));
                }
            }

            return new ThreadedServer(_Companion, config, extensions, _Router);
        }

        #endregion

    }

}
