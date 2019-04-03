using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Core.Infrastructure.Configuration;

namespace GenHTTP.Core.Infrastructure.Endpoints
{

    internal class EndPointCollection : IDisposable
    {

        #region Get-/Setters

        private IServer Server { get; }

        private List<EndPoint> EndPoints { get; }

        private NetworkConfiguration NetworkConfiguration { get; }

        #endregion

        #region Initialization

        public EndPointCollection(IServer server, IEnumerable<EndPointConfiguration> configuration, NetworkConfiguration networkConfiguration)
        {
            Server = server;
            NetworkConfiguration = networkConfiguration;

            EndPoints = new List<EndPoint>();

            foreach (var config in configuration)
            {
                EndPoints.Add(Start(config));
            }
        }

        #endregion

        #region Functionality

        private EndPoint Start(EndPointConfiguration configuration)
        {
            var endpoint = new IPEndPoint(configuration.Address, configuration.Port);

            if (configuration.Security == null)
            {
                return new InsecureEndPoint(Server, endpoint, NetworkConfiguration);
            }
            else
            {
                return new SecureEndPoint(Server, endpoint, configuration.Security, NetworkConfiguration);
            }
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
                    foreach (var endpoint in EndPoints)
                    {
                        try
                        {
                            endpoint.Dispose();
                        }
                        catch (Exception e)
                        {
                            Server.Companion?.OnServerError(ServerErrorScope.ServerConnection, e);
                        }
                    }

                    EndPoints.Clear();
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
