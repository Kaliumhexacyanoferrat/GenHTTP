using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Routing;

using GenHTTP.Core.Routing;
using GenHTTP.Core.Infrastructure.Configuration;
using GenHTTP.Core.Infrastructure.Endpoints;

namespace GenHTTP.Core.Infrastructure
{

    internal class ThreadedServer : IServer
    {

        #region Get-/Setters

        public Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        public IRouter Router { get; private set; }

        public IServerCompanion? Companion { get; private set; }

        public IExtensionCollection Extensions { get; }

        internal ServerConfiguration Configuration { get; }

        internal EndPointCollection EndPoints { get; }

        #endregion

        #region Constructors

        internal ThreadedServer(IServerCompanion? companion, ServerConfiguration configuration, IExtensionCollection extensions, IRouter router)
        {
            Companion = companion;
            Router = new CoreRouter(router);

            Extensions = extensions;
            Configuration = configuration;

            EndPoints = new EndPointCollection(this, configuration.EndPoints, configuration.Network);
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
                    EndPoints.Dispose();
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

    }

}
