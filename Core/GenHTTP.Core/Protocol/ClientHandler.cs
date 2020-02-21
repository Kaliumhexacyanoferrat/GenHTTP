using System;
using System.IO;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Core.Infrastructure.Configuration;
using GenHTTP.Core.Protocol;

namespace GenHTTP.Core
{

    internal class ClientHandler
    {

        #region Get-/Setter

        public IServer Server { get; }

        public IEndPoint EndPoint { get; }

        internal NetworkConfiguration Configuration { get; }

        internal Socket Connection { get; }

        internal Stream Stream { get; }

        private bool? KeepAlive { get; set; }

        #endregion

        #region Initialization

        internal ClientHandler(Socket socket, Stream stream, IServer server, IEndPoint endPoint, NetworkConfiguration config)
        {
            Server = server;
            EndPoint = endPoint;

            Configuration = config;
            Connection = socket;

            Stream = stream;
        }

        #endregion

        #region Functionality

        internal async Task Run()
        {
            try
            {
                var options = new StreamPipeReaderOptions(leaveOpen: true);

                await HandlePipe(PipeReader.Create(Stream, options));
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

        private async Task HandlePipe(PipeReader reader)
        {
            try
            {
                var buffer = new RequestBuffer(reader, Configuration);

                var parser = new RequestParser(Configuration);

                RequestBuilder? request;

                while ((request = await parser.TryParseAsync(buffer)) != null)
                {
                    if (!await HandleRequest(request))
                    {
                        break;
                    }
                }
            }
            finally
            {
                await reader.CompleteAsync();
            }
        }

        private async Task<bool> HandleRequest(RequestBuilder builder)
        {
            var address = ((IPEndPoint)Connection.RemoteEndPoint).Address;

            using (var request = builder.Connection(Server, EndPoint, address).Build())
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
