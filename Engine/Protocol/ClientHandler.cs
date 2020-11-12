using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Infrastructure.Configuration;
using GenHTTP.Engine.Protocol;

using PooledAwait;

namespace GenHTTP.Engine
{

    internal class ClientHandler
    {
        private static readonly StreamPipeReaderOptions READER_OPTIONS = new StreamPipeReaderOptions(pool: MemoryPool<byte>.Shared, leaveOpen: true);

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

        internal async PooledValueTask Run()
        {
            try
            {
                await HandlePipe(PipeReader.Create(Stream, READER_OPTIONS)).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Server.Companion?.OnServerError(ServerErrorScope.ClientConnection, e);
            }
            finally
            {
                try
                {
                    await Stream.DisposeAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Server.Companion?.OnServerError(ServerErrorScope.ClientConnection, e);
                }

                try
                {
                    if (Connection.Connected)
                    {
                        Connection.Shutdown(SocketShutdown.Both);
                        Connection.Disconnect(false);
                        Connection.Close();
                    }

                    Connection.Dispose();
                }
                catch (Exception e)
                {
                    Server.Companion?.OnServerError(ServerErrorScope.ClientConnection, e);
                }
            }
        }

        private async PooledValueTask HandlePipe(PipeReader reader)
        {
            try
            {
                using var buffer = new RequestBuffer(reader, Configuration);

                var parser = new RequestParser(Configuration);

                RequestBuilder? request;

                while ((request = await parser.TryParseAsync(buffer).ConfigureAwait(false)) != null)
                {
                    if (!await HandleRequest(request).ConfigureAwait(false))
                    {
                        break;
                    }
                }
            }
            finally
            {
                await reader.CompleteAsync().ConfigureAwait(false);
            }
        }

        private async ValueTask<bool> HandleRequest(RequestBuilder builder)
        {
            var address = (Connection.RemoteEndPoint as IPEndPoint)?.Address;

            using var request = builder.Connection(Server, EndPoint, address).Build();
            
            if (KeepAlive == null)
            {
                KeepAlive = request["Connection"]?.Equals("Keep-Alive", StringComparison.InvariantCultureIgnoreCase) ?? false;
            }

            bool keepAlive = (bool)KeepAlive;

            var responseHandler = new ResponseHandler(Server, Stream, Connection, Configuration);

            using var response = await Server.Handler.HandleAsync(request).ConfigureAwait(false) ?? throw new InvalidOperationException("The root request handler did not return a response");
            
            var success = await responseHandler.Handle(request, response, keepAlive).ConfigureAwait(false);

            if (!success || !keepAlive)
            {
                Connection.Shutdown(SocketShutdown.Both);
                Connection.Disconnect(false);
                Connection.Close();

                return false;
            }

            return true;
        }

        #endregion

    }

}
