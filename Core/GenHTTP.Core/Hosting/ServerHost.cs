using System;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Routing;

namespace GenHTTP.Core.Hosting
{

    public class ServerHost : IServerHost
    {
        private readonly IServerBuilder _Builder = Server.Create();

        #region Builder facade

        public IServerHost Backlog(ushort backlog) { _Builder.Backlog(backlog); return this; }

        public IServerHost Bind(IPAddress address, ushort port) { _Builder.Bind(address, port); return this; }

        public IServerHost Bind(IPAddress address, ushort port, string host, X509Certificate2 certificate) { _Builder.Bind(address, port, host, certificate); return this; }

        public IServerHost Bind(IPAddress address, ushort port, string host, X509Certificate2 certificate, SslProtocols protocols) { _Builder.Bind(address, port, host, certificate, protocols); return this; }

        public IServerHost Bind(IPAddress address, ushort port, ICertificateProvider certificateProvider) { _Builder.Bind(address, port, certificateProvider); return this; }

        public IServerHost Bind(IPAddress address, ushort port, ICertificateProvider certificateProvider, SslProtocols protocols) { _Builder.Bind(address, port, certificateProvider, protocols); return this; }

        public IServerHost Companion(IServerCompanion companion) { _Builder.Companion(companion); return this; }

        public IServerHost Compression(IBuilder<ICompressionAlgorithm> algorithm) { _Builder.Compression(algorithm); return this; }

        public IServerHost Compression(ICompressionAlgorithm algorithm) { _Builder.Compression(algorithm); return this; }

        public IServerHost Compression(bool enabled) { _Builder.Compression(enabled); return this; }

        public IServerHost Console() { _Builder.Console(); return this; }

        public IServerHost Development(bool developmentMode = true) { _Builder.Development(developmentMode); return this; }

        public IServerHost Extension(IServerExtensionBuilder extension) { _Builder.Extension(extension); return this; }

        public IServerHost Extension(IServerExtension extension) { _Builder.Extension(extension); return this; }

        public IServerHost Port(ushort port) { _Builder.Port(port); return this; }

        public IServerHost RequestMemoryLimit(uint limit) { _Builder.RequestMemoryLimit(limit); return this; }

        public IServerHost RequestReadTimeout(TimeSpan timeout) { _Builder.RequestReadTimeout(timeout); return this; }

        public IServerHost Router(IRouterBuilder routerBuilder) { _Builder.Router(routerBuilder); return this; }

        public IServerHost Router(IRouter router) { _Builder.Router(router); return this; }

        public IServerHost SecureUpgrade(SecureUpgrade upgradeMode) { _Builder.SecureUpgrade(upgradeMode); return this; }

        public IServerHost StrictTransport(TimeSpan maximumAge, bool includeSubdomains = true, bool preload = true) { _Builder.StrictTransport(maximumAge, includeSubdomains, preload); return this; }

        public IServerHost StrictTransport(bool enabled) { _Builder.StrictTransport(enabled); return this; }

        public IServerHost TransferBufferSize(uint bufferSize) { _Builder.TransferBufferSize(bufferSize); return this; }

        public IServer Build() => _Builder.Build();

        #endregion

        #region Get-/Setters

        public IServer? Instance { get; private set; }

        #endregion

        #region Functionality

        public int Run()
        {
#if !DEBUG
            try
            {
#endif
            using var waitEvent = new AutoResetEvent(false);

            AppDomain.CurrentDomain.ProcessExit += (_, __) =>
            {
                waitEvent.Set();
            };

            Start();

            try
            {
                waitEvent.WaitOne();
            }
            finally
            {
                Stop();
            }

            return 0;
#if !DEBUG
            }
            catch
            {
                return -1;
            }
#endif
        }

        public IServerHost Start()
        {
            Stop();

            Instance = Build();
            return this;
        }

        public IServerHost Stop()
        {
            Instance?.Dispose();
            Instance = null;

            return this;
        }

        public IServerHost Restart()
        {
            Stop();
            Start();

            return this;
        }

        #endregion

    }

}
