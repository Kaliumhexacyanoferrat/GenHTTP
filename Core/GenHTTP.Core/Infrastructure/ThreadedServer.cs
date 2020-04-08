using System;
using System.Reflection;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Core.Infrastructure.Configuration;
using GenHTTP.Core.Infrastructure.Endpoints;

namespace GenHTTP.Core.Infrastructure
{

    internal class ThreadedServer : IServer
    {
        private readonly EndPointCollection _EndPoints;

        #region Get-/Setters

        public string Version { get; }

        public bool Development => Configuration.DevelopmentMode;

        public IHandler Handler { get; private set; }

        public IServerCompanion? Companion { get; private set; }

        public IEndPointCollection EndPoints => _EndPoints;

        internal ServerConfiguration Configuration { get; }

        #endregion

        #region Constructors

        internal ThreadedServer(IServerCompanion? companion, ServerConfiguration configuration, IHandler handler)
        {
            Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            Companion = companion;
            Configuration = configuration;

            _EndPoints = new EndPointCollection(this, configuration.EndPoints, configuration.Network);

            Handler = handler;
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
