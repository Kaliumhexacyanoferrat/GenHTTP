using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

using GenHTTP.Api.Protocol;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Core.Protocol;

namespace GenHTTP.Core
{

    internal class ClientHandler : IClientHandler
    {

        #region Get-/Setter

        public IServer Server { get; }

        public IPAddress IPAddress => ((IPEndPoint)Connection.RemoteEndPoint).Address;

        internal Socket Connection { get; }

        internal NetworkStream NetworkStream { get; }

        protected Parser Parser { get; }

        protected bool? KeepAlive { get; set; }

        #endregion

        #region Initialization

        internal ClientHandler(Socket socket, IServer server)
        {
            Server = server;

            Connection = socket;
            NetworkStream = new NetworkStream(socket, false);

            Parser = new Parser(socket, server, HandleRequest);
        }

        #endregion

        #region Functionality

        internal void Run(object state)
        {
            try
            {
                Parser.Run();
            }
            catch (Exception e)
            {
                Server.Companion?.OnServerError(ServerErrorScope.ClientConnection, e);
            }
            finally
            {
                try
                {
                    if (Connection.Connected)
                    {
                        Connection.Disconnect(false);
                        Connection.Close();

                        Connection.Dispose();

                        NetworkStream.Dispose();
                    }
                }
                catch (Exception e)
                {
                    Server.Companion?.OnServerError(ServerErrorScope.ClientConnection, e);
                }
            }
        }

        private void HandleRequest(RequestBuilder builder)
        {
            using (var request = builder.Handler(this).Build())
            {
                if (KeepAlive == null)
                {
                    KeepAlive = request["Connection"]?.Equals("Keep-Alive", StringComparison.InvariantCultureIgnoreCase) ?? false;
                }

                bool keepAlive = (bool)KeepAlive;

                var responseHandler = new ResponseHandler(Server, NetworkStream);
                var requestHandler = new RequestHandler(Server);

                using (var response = requestHandler.Handle(request, out Exception? error))
                {
                    var success = responseHandler.Handle(request, response, keepAlive, error);

                    if (!success || !keepAlive)
                    {
                        Connection.LingerState = new LingerOption(true, 1);
                        Connection.Disconnect(false);
                        Connection.Close();
                    }
                }
            }

        }

        #endregion

    }

}
