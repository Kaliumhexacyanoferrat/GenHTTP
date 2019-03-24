using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Protocol.Exceptions;
using GenHTTP.Api.Routing;
using GenHTTP.Core.Routing;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Core
{

    /// <summary>
    /// A small webserver written in C# which allows you to run your
    /// own web applications.
    /// </summary>
    [Serializable]
    public class Server : IServer
    {

        // State
        private bool _Exit = false;
        
        #region Events

        /// <summary>
        /// You can subscribe to this event if you want to get notified whenever a request was sucessfully handled by a project.
        /// </summary>
        public event RequestHandled OnRequestHandled;
        
        /// <summary>
        /// Allow other applications to analyze requests,
        /// </summary>
        /// <param name="request">The request which got handled</param>
        /// <param name="response">The response which was sent</param>
        internal void CallCompletionEvent(IHttpRequest request, IHttpResponse response)
        {
            if (request != null && response != null && OnRequestHandled != null) OnRequestHandled(request, response);
        }

        #endregion

        #region Constructors

        public Server(IRouter router, ILoggerFactory loggerFactory, int port = 80, int backlog = 20)
        {
            Router = new CoreRouter(this, router);
            
            LoggerFactory = loggerFactory;

            Log = loggerFactory.CreateLogger<Server>();

            Port = port;
            Backlog = backlog;

            Listen();
        }

        #endregion

        #region Get-/Setters

        /// <summary>
        /// The version of the server software.
        /// </summary>
        public Version Version => new Version(2, 0, 0);

        public int Port { get; protected set; }

        public int Backlog { get; protected set; }

        public ILoggerFactory LoggerFactory { get; protected set; }

        public IRouter Router { get; protected set; }

        protected ILogger Log { get; set; }

        protected Socket Socket { get; private set; }

        protected Thread MainThread { get; private set; }
        
        #endregion

        #region Functionality

        /// <summary>
        /// Listen on the given port for incoming connections.
        /// </summary>
        private void Listen()
        {
            // headline
            Log.LogInformation($"GenHTTP v{Version}");
            
            // init socket
            try
            {
                Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);

                Socket.Bind(new IPEndPoint(IPAddress.Any, Port));
                Socket.Listen(Backlog);

                Log.LogInformation($"Server running on port {Port} ...");
            }
            catch (Exception e)
            {
                Socket = null;

                throw new SocketBindingException($"Failed to bind to port {Port}.", e);
            }

            // start the accept loop
            MainThread = new Thread(MainLoop);
            MainThread.Start();
        }

        private void MainLoop()
        {
            try
            {
                do
                {
                    var clientSocket = Socket.Accept();
                    var handler = new ClientHandler(clientSocket, this);

                    ThreadPool.QueueUserWorkItem(new WaitCallback(handler.Run));
                } while (!_Exit);
            }
            catch { }
        }

        #endregion

        #region Helpers
        
        internal void LogRequest(HttpRequest request, HttpResponse response)
        {
            // ToDo

            /*lock (_Log)
            {
                _Log.WriteTimestamp();
                _Log.WriteColored(response.ClientHandler.IP.PadRight(14, ' '), ConsoleColor.Yellow);
                string status = Mapping.GetStatusCode(response.Header.Type);
                if (status.StartsWith("4") || status.StartsWith("5"))
                {
                    _Log.WriteColored(" " + Mapping.GetStatusCode(response.Header.Type), ConsoleColor.Red);
                }
                else
                {
                    _Log.WriteColored(" " + Mapping.GetStatusCode(response.Header.Type), ConsoleColor.Green);
                }
                _Log.WriteColored(" " + HttpRequest.GetRequestTypeName(request.Type), ConsoleColor.White);
                _Log.WriteColored(" " + request.File, ConsoleColor.Gray);
                _Log.WriteRightAlign(response.ContentLenght.ToString().PadLeft(5, ' '), ConsoleColor.DarkMagenta);
            }*/
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
                    try
                    {
                        MainThread.Abort();
                    }
                    catch
                    {
                        MainThread = null;
                    }

                    try
                    {
                        Socket.Dispose();
                    }
                    catch
                    {
                        Socket = null;
                    }
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
