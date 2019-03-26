using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol.Exceptions;
using GenHTTP.Api.Routing;

using GenHTTP.Core.Routing;

namespace GenHTTP.Core
{

    /// <summary>
    /// A small webserver written in C# which allows you to run your
    /// own web applications.
    /// </summary>
    public class Server : IServer
    {

        #region Get-/Setters

        public Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        public IRouter Router { get; protected set; }

        public IServerCompanion? Companion { get; protected set; }

        protected Socket Socket { get; private set; }

        protected Thread MainThread { get; private set; }

        #endregion

        #region Constructors

        public Server(IRouter router, IServerCompanion? companion, int port = 80, int backlog = 20)
        {
            Companion = companion;
            Router = new CoreRouter(this, router);

            try
            {
                Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);

                Socket.Bind(new IPEndPoint(IPAddress.Any, port));
                Socket.Listen(backlog);
            }
            catch (Exception e)
            {
                throw new SocketBindingException($"Failed to bind to port {port}.", e);
            }

            MainThread = new Thread(MainLoop);
            MainThread.Start();
        }

        #endregion

        #region Functionality

        private void MainLoop()
        {
            try
            {
                do
                {
                    var clientSocket = Socket.Accept();
                    var handler = new ClientHandler(clientSocket, this);

                    ThreadPool.QueueUserWorkItem(new WaitCallback(handler.Run));
                }
                while (true);
            }
            catch (Exception e)
            {
                Companion?.OnServerError(ServerErrorScope.ServerConnection, e);
            }
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
                    catch (Exception e)
                    {
                        Companion?.OnServerError(ServerErrorScope.ServerConnection, e);
                    }

                    try
                    {
                        Socket.Dispose();
                    }
                    catch (Exception e)
                    {
                        Companion?.OnServerError(ServerErrorScope.ServerConnection, e);
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
