using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using GenHTTP.Api.Protocol;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Core.Protocol;
using GenHTTP.Core.Infrastructure;

namespace GenHTTP.Core
{

    internal class ClientHandler : IClientHandler
    {

        #region Get-/Setter

        public IServer Server { get; }

        public IPAddress IPAddress => ((IPEndPoint)Connection.RemoteEndPoint).Address;

        internal NetworkConfiguration Configuration { get; }

        internal Socket Connection { get; }

        internal NetworkStream NetworkStream { get; }

        protected RequestParser Parser { get; }

        protected bool? KeepAlive { get; set; }

        #endregion

        #region Initialization

        internal ClientHandler(Socket socket, IServer server, NetworkConfiguration config)
        {
            Server = server;

            Configuration = config;
            Connection = socket;

            NetworkStream = new NetworkStream(socket, false)
            {
                ReadTimeout = (int)config.RequestReadTimeout.TotalMilliseconds
            };

            Parser = new RequestParser(NetworkStream, Configuration);
        }

        #endregion

        #region Functionality

        internal async Task Run()
        {
            try
            {
                bool closeConnection = false;

                do
                {
                    var request = await Parser.GetRequest();
                    closeConnection = await HandleRequest(request);
                }
                while (!closeConnection);
            }
            catch (Exception e)
            {
                if (!(e is ReadTimeoutException))
                {
                    Server.Companion?.OnServerError(ServerErrorScope.ClientConnection, e);
                }
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

        private async Task<bool> HandleRequest(RequestBuilder builder)
        {
            using (var request = builder.Handler(this).Build())
            {
                if (KeepAlive == null)
                {
                    KeepAlive = request["Connection"]?.Equals("Keep-Alive", StringComparison.InvariantCultureIgnoreCase) ?? false;
                }

                bool keepAlive = (bool)KeepAlive;

                var responseHandler = new ResponseHandler(Server, NetworkStream, Configuration);
                var requestHandler = new RequestHandler(Server);

                using (var response = requestHandler.Handle(request, out Exception? error))
                {
                    var success = await responseHandler.Handle(request, response, keepAlive, error);

                    if (!success || !keepAlive)
                    {
                        Connection.LingerState = new LingerOption(true, 1);
                        Connection.Disconnect(false);
                        Connection.Close();

                        return true;
                    }

                    return false;
                }
            }
        }

        #endregion

    }

}
