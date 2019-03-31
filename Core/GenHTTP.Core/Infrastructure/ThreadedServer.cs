using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

using GenHTTP.Core.Routing;

namespace GenHTTP.Core.Infrastructure
{

    internal class ThreadedServer : IServer
    {

        #region Get-/Setters

        public Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        public IRouter Router { get; protected set; }

        public IServerCompanion? Companion { get; protected set; }

        internal ServerConfiguration Configuration { get; }

        protected Socket Socket { get; private set; }

        protected Thread MainThread { get; private set; }

        #endregion

        #region Constructors

        internal ThreadedServer(IRouter router, IServerCompanion? companion, ServerConfiguration configuration)
        {
            Companion = companion;
            Router = new CoreRouter(router);

            Configuration = configuration;

            try
            {
                Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);

                Socket.Bind(new IPEndPoint(IPAddress.Any, Configuration.Port));
                Socket.Listen(Configuration.Backlog);
            }
            catch (Exception e)
            {
                throw new BindingException($"Failed to bind to port {Configuration.Port}.", e);
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
                    var handler = new ClientHandler(clientSocket, this, Configuration.Network);

                    ThreadPool.QueueUserWorkItem(new WaitCallback(async (_) => await handler.Run()));
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
