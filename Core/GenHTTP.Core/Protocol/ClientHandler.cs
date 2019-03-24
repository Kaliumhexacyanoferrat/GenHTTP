using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

using GenHTTP.Api.Protocol;
using GenHTTP.Api.Infrastructure;

using Microsoft.Extensions.Logging;

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
            _Server = server;

            Log = server.LoggerFactory.CreateLogger<ClientHandler>();
        }

        #endregion

        #region Get-/Setter

        protected ILogger Log { get; }

        protected Socket Connection { get; }

        /// <summary>
        /// The server this handler relates to.
        /// </summary>
        public IServer Server => _Server;

        /// <summary>
        /// Time span between handling the request and sending the response.
        /// </summary>
        public TimeSpan? LoadTime { get; private set; }

        /// <summary>
        /// The IP of the connected client.
        /// </summary>
        public IPAddress IPAddress => ((IPEndPoint)Connection.RemoteEndPoint).Address;

        #endregion

        #region Functionality

        /// <summary>
        /// Begin to handle the client's requests.
        /// </summary>
        internal void Run(object state)
        {
            try
            {
                // run parser
                HttpParser parser = new HttpParser(Connection, _Server, this);
                parser.Run();
            }
            catch { }
        }

        internal bool HandleRequest(HttpRequest request, bool keepAlive)
        {
            var response = new HttpResponse(this, request.Type == RequestType.HEAD, request.ProtocolType, keepAlive);

            try
            {
                var routing = Server.Router.GetContext(request);
                request.Routing = routing;

                try
                {
                    var provider = routing.ContentProvider ?? routing.Router.GetProvider(ResponseType.NotFound, routing);

                    provider.Handle(request, response);

                    if (!response.Sent)
                    {
                        routing.Router.GetProvider(ResponseType.InternalServerError, routing)
                                      .Handle(request, response);
                    }
                }
                catch (Exception e)
                {
                    Log.LogError(e, $"Error while handling request '{request.Path}'");

                    routing.Router.GetProvider(ResponseType.InternalServerError, routing)
                                  .Handle(request, response);
                }
            }
            catch (Exception e)
            {
                Log.LogError(e, "Unable to handle request.");
                return true;
            }

            _Server.CallCompletionEvent(request, response);

            if (response.Header.CloseConnection)
            {
                return true;
            }

            return !keepAlive;
        }

        internal void Send(Stream content)
        {
            // ToDo: Rework this to be async and not wasting resources like hell
            try
            {
                using (var mem = new MemoryStream())
                {
                    content.CopyTo(mem);
                    mem.Seek(0, SeekOrigin.Begin);

                    Connection.Send(mem.ToArray());
                }
            }
            catch
            {
                // ToDo: ??
            }
        }

        internal void SendBytes(byte[] content)
        {
            Connection.Send(content);
        }

        #endregion

    }

}
