using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
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
            var authenticated = TryAuthenticate(client, out var stream);

            if (authenticated && stream != null)
            {
                await Handle(client, stream);
            }
            else
            {
                try
                {
                    client.Close();
                    client.Dispose();
                }
                catch (Exception e)
                {
                    Server.Companion?.OnServerError(ServerErrorScope.ClientConnection, e);
                }
            }
        }

        private bool TryAuthenticate(Socket client, out SslStream? stream)
        {
            var readAhead = new ReadAheadStream(new NetworkStream(client));

            try
            {
                if (readAhead.Peek())
                {
                    var host = GetHostName(readAhead, Options.Certificate.SupportedHosts);
                    
                    stream = new SslStream(readAhead, false);

                    var certificate = Options.Certificate.Provide(host);

                    stream.AuthenticateAsServer(certificate, false, Options.Protocols, true);

                    return true;
                }

                stream = null;
                return false;
            }
            catch (Exception e)
            {
                readAhead.Dispose();

                Server.Companion?.OnServerError(ServerErrorScope.Security, e);

                stream = null;
                return false;
            }
        }

        #endregion

        #region Client Hello (very hacky at the moment ...)
        
        private string? GetHostName(ReadAheadStream stream, IEnumerable<string> supportedHosts)
        {
            var str = Encoding.ASCII.GetString(stream.Buffer.ToArray());

            foreach (var host in supportedHosts)
            {
                if (str.Contains(host))
                {
                    return host;
                }
            }

            return null;
        }

        #endregion

    }

}
