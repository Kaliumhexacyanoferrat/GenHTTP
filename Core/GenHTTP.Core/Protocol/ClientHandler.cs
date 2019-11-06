using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Core.Protocol;
using GenHTTP.Core.Infrastructure.Configuration;

namespace GenHTTP.Core
{

    internal class ClientHandler : IClient
    {

        #region Get-/Setter

        public IServer Server { get; }

        public IEndPoint EndPoint { get; }

        internal NetworkConfiguration Configuration { get; }

        internal Socket Connection { get; }

        internal Stream Stream { get; }

        private RequestParser Parser { get; }

        private bool? KeepAlive { get; set; }

        #endregion

        #region Information

        public IPAddress IPAddress => ((IPEndPoint)Connection.RemoteEndPoint).Address;

        public ushort Port => (ushort)((IPEndPoint)Connection.RemoteEndPoint).Port;

        #endregion

        #region Initialization

        internal ClientHandler(Socket socket, Stream stream, IServer server, IEndPoint endPoint, NetworkConfiguration config)
        {
            Server = server;
            EndPoint = endPoint;

            Configuration = config;
            Connection = socket;

            Stream = stream;

            Parser = new RequestParser(Stream, Configuration);
        }

        #endregion

        #region Functionality

        internal async Task Run()
        {
            try
            {
                var closeConnection = false;

                do
                {
                    var request = await Parser.GetRequest();

                    if (request != null)
                    {
                        closeConnection = await HandleRequest(request);
                    }
                    else
                    {
                        closeConnection = true;
                    }
                }
                while (!closeConnection);
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
                    }

                    Connection.Dispose();

                    Stream.Dispose();
                }
                catch (Exception e)
                {
                    Server.Companion?.OnServerError(ServerErrorScope.ClientConnection, e);
                }
            }
        }

        private async Task<bool> HandleRequest(RequestBuilder builder)
        {
            using (var request = builder.Connection(Server, EndPoint, this).Build())
            {
                if (KeepAlive == null)
                {
                    KeepAlive = request["Connection"]?.Equals("Keep-Alive", StringComparison.InvariantCultureIgnoreCase) ?? false;
                }

                bool keepAlive = (bool)KeepAlive;

                var responseHandler = new ResponseHandler(Server, Stream, Configuration);
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
