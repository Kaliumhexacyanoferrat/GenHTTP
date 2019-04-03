using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading.Tasks;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Core.Infrastructure.Configuration;

namespace GenHTTP.Core.Infrastructure.Endpoints
{

    internal class SecureEndPoint : EndPoint
    {

        #region Get-/Setters

        internal SecurityConfiguration Options { get; }

        public override bool Secure => true;

        #endregion

        #region Initialization

        internal SecureEndPoint(IServer server, IPEndPoint endPoint, SecurityConfiguration options, NetworkConfiguration configuration)
            : base(server, endPoint, configuration)
        {
            Options = options;
        }

        #endregion

        #region Functionality

        protected override async Task Accept(Socket client)
        {
            if (TryAuthenticate(client, out var stream))
            {
                await Handle(client, stream);
            }
            else
            {
                try
                {
                    stream.Dispose();

                    client.Close();
                    client.Dispose();
                }
                catch (Exception e)
                {
                    Server.Companion?.OnServerError(ServerErrorScope.ClientConnection, e);
                }
            }
        }

        private bool TryAuthenticate(Socket client, out SslStream stream)
        {
            stream = new SslStream(new NetworkStream(client), false);
            
            try
            {
                stream.AuthenticateAsServer(Options.Certificate, false, Options.Protocols, true);
                return true;
            }
            catch (Exception e)
            {
                Server.Companion?.OnServerError(ServerErrorScope.Security, e);
                return false;
            }
        }

        #endregion

    }

}
