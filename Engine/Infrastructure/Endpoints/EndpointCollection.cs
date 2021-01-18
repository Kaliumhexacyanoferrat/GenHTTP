using System;
using System.Collections.Generic;
using System.Net;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Engine.Infrastructure.Configuration;

namespace GenHTTP.Engine.Infrastructure.Endpoints
{

    internal sealed class EndPointCollection : List<IEndPoint>, IDisposable, IEndPointCollection
    {

        #region Get-/Setters

        private IServer Server { get; }

        private NetworkConfiguration NetworkConfiguration { get; }

        #endregion

        #region Initialization

        public EndPointCollection(IServer server, IEnumerable<EndPointConfiguration> configuration, NetworkConfiguration networkConfiguration)
        {
            Server = server;
            NetworkConfiguration = networkConfiguration;

            foreach (var config in configuration)
            {
                Add(Start(config));
            }
        }

        #endregion

        #region Functionality

        private EndPoint Start(EndPointConfiguration configuration)
        {
            var endpoint = new IPEndPoint(configuration.Address, configuration.Port);

            if (configuration.Security is null)
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

        public void Dispose()
        {
            if (!disposed)
            {
                foreach (IEndPoint endpoint in this)
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

                Clear();

                disposed = true;
            }

            GC.SuppressFinalize(this);
        }

        #endregion

    }

}
