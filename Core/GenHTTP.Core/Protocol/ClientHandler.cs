using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

using GenHTTP.Api.Protocol;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Core
{

    /// <summary>
    /// Handles the requests from a browser.
    /// </summary>
    public class ClientHandler : IClientHandler
    {
        private Server _Server;

        #region Initialization

        /// <summary>
        /// Create a new instance to handle a request from a client.
        /// </summary>
        /// <param name="socket">The socket to read from</param>
        /// <param name="server">The server this handler relates to</param>
        public ClientHandler(Socket socket, Server server)
        {
            Connection = socket;
            NetworkStream = new NetworkStream(socket, false);

            _Server = server;
        }

        #endregion

        #region Get-/Setter

        /// <summary>
        /// The server this handler relates to.
        /// </summary>
        public IServer Server => _Server;

        /// <summary>
        /// The IP of the connected client.
        /// </summary>
        public IPAddress IPAddress => ((IPEndPoint)Connection.RemoteEndPoint).Address;

        protected Socket Connection { get; }

        protected NetworkStream NetworkStream { get; }

        #endregion

        #region Functionality

        /// <summary>
        /// Begin to handle the client's requests.
        /// </summary>
        internal void Run(object state)
        {
            try
            {
                new HttpParser(Connection, _Server, this).Run();
            }
            catch(Exception e)
            {
                Server.Companion?.OnServerError(ServerErrorScope.ClientConnection, e);
            }
            finally
            {
                try
                {
                    NetworkStream.Dispose();
                }
                catch(Exception e)
                {
                    Server.Companion?.OnServerError(ServerErrorScope.ClientConnection, e);
                }
            }
        }

        internal void Send(Stream content)
        {
            try
            {
                content.CopyTo(NetworkStream);
            }
            catch (Exception e)
            {
                Server.Companion?.OnServerError(ServerErrorScope.ClientConnection, e);
            }
        }

        internal void Send(byte[] content)
        {
            NetworkStream.Write(content, 0, content.Length);
        }

        #endregion
        
    }

}
