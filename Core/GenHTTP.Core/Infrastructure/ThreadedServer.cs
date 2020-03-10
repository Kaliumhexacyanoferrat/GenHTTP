using System;
using System.Reflection;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Routing;
using GenHTTP.Core.Infrastructure.Configuration;
using GenHTTP.Core.Infrastructure.Endpoints;
using GenHTTP.Core.Routing;
using GenHTTP.Modules.Core;
using GenHTTP.Modules.Core.Websites;

namespace GenHTTP.Core.Infrastructure
{

    internal class ThreadedServer : IServer
    {
        private readonly EndPointCollection _EndPoints;

        #region Get-/Setters

        public string Version { get; }

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
            Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            Companion = companion;

            var coreWebsite = Website.Create()
                                     .Content(router)
                                     .Build();

            Router = new CoreRouter(coreWebsite);

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
