using System;
using System.Reflection;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Routing;

using GenHTTP.Core.Routing;
using GenHTTP.Core.Infrastructure.Configuration;
using GenHTTP.Core.Infrastructure.Endpoints;

namespace GenHTTP.Core.Infrastructure
{

    internal class ThreadedServer : IServer
    {
        private readonly EndPointCollection _EndPoints;

        #region Get-/Setters

        public Version Version { get; }

        public bool Development => Configuration.DevelopmentMode;

        public IRouter Router { get; private set; }

        public IServerCompanion? Companion { get; private set; }

        public IExtensionCollection Extensions { get; }

        public IEndPointCollection EndPoints => _EndPoints;

        internal ServerConfiguration Configuration { get; }

        #endregion

        #region Constructors

        internal ThreadedServer(IServerCompanion? companion, ServerConfiguration configuration, IExtensionCollection extensions, IRouter router)
        {
            Version = Assembly.GetExecutingAssembly().GetName().Version;

            Companion = companion;
            Router = new CoreRouter(router);

            Extensions = extensions;
            Configuration = configuration;

            _EndPoints = new EndPointCollection(this, configuration.EndPoints, configuration.Network);
        }

        #endregion

        #region IDisposable Support

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _EndPoints.Dispose();
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

    }

}
