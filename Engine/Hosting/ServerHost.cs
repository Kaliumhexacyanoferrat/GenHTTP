using System;
using System.Diagnostics;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Engine.Hosting
{

    public class ServerHost : IServerHost
    {
        private readonly IServerBuilder _Builder = Server.Create();

        #region Builder facade

        public IServerHost Backlog(ushort backlog) { _Builder.Backlog(backlog); return this; }

        public IServerHost Bind(IPAddress address, ushort port) { _Builder.Bind(address, port); return this; }

        public IServerHost Bind(IPAddress address, ushort port, X509Certificate2 certificate) { _Builder.Bind(address, port, certificate); return this; }

        public IServerHost Bind(IPAddress address, ushort port, X509Certificate2 certificate, SslProtocols protocols) { _Builder.Bind(address, port, certificate, protocols); return this; }

        public IServerHost Bind(IPAddress address, ushort port, ICertificateProvider certificateProvider) { _Builder.Bind(address, port, certificateProvider); return this; }

        public IServerHost Bind(IPAddress address, ushort port, ICertificateProvider certificateProvider, SslProtocols protocols) { _Builder.Bind(address, port, certificateProvider, protocols); return this; }

        public IServerHost Companion(IServerCompanion companion) { _Builder.Companion(companion); return this; }

        public IServerHost Console() { _Builder.Console(); return this; }

        public IServerHost Development(bool developmentMode = true) { _Builder.Development(developmentMode); return this; }

        public IServerHost Port(ushort port) { _Builder.Port(port); return this; }

        public IServerHost RequestMemoryLimit(uint limit) { _Builder.RequestMemoryLimit(limit); return this; }

        public IServerHost RequestReadTimeout(TimeSpan timeout) { _Builder.RequestReadTimeout(timeout); return this; }

        public IServerHost Handler(IHandlerBuilder handler) { _Builder.Handler(handler); return this; }

        public IServerHost TransferBufferSize(uint bufferSize) { _Builder.TransferBufferSize(bufferSize); return this; }

        public IServerHost Add(IConcernBuilder concern) { _Builder.Add(concern); return this; }

        public IServer Build() => _Builder.Build();

        #endregion

        #region Get-/Setters

        public IServer? Instance { get; private set; }

        #endregion

        #region Functionality

        public int Run()
        {
            try
            {
                var waitEvent = new AutoResetEvent(false);

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
            }
            catch (Exception e)
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }

                var companion = Instance?.Companion;

                if (companion != null)
                {
                    companion.OnServerError(ServerErrorScope.General, e);
                }
                else
                {
                    System.Console.WriteLine(e);
                }

                return -1;
            }
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
