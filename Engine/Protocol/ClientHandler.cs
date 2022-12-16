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

namespace GenHTTP.Engine
{

    /// <summary>
    /// Maintains a single connection to a client, continuously reading
    /// requests and generating responses.
    /// </summary>
    /// <remarks>
    /// Implements keep alive and maintains the connection state (e.g. by
    /// closing it after the last request has been handled).
    /// </remarks>
    internal sealed class ClientHandler
    {
        private static readonly StreamPipeReaderOptions READER_OPTIONS = new(pool: MemoryPool<byte>.Shared, leaveOpen: true, bufferSize: 65536);

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

        internal async ValueTask Run()
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
                    await Stream.DisposeAsync();
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

        private async ValueTask HandlePipe(PipeReader reader)
        {
            try
            {
                using var buffer = new RequestBuffer(reader, Configuration);

                var parser = new RequestParser(Configuration);

                RequestBuilder? request;

                while ((request = await parser.TryParseAsync(buffer).ConfigureAwait(false)) is not null)
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

        private async ValueTask<bool> HandleRequest(RequestBuilder builder)
        {
            var address = (Connection.RemoteEndPoint as IPEndPoint)?.Address;

            using var request = builder.Connection(Server, EndPoint, address).Build();
            
            if (KeepAlive is null)
            {
                KeepAlive = request["Connection"]?.Equals("Keep-Alive", StringComparison.InvariantCultureIgnoreCase) ?? true;
            }

            bool keepAlive = (bool)KeepAlive;

            var responseHandler = new ResponseHandler(Server, Stream, Connection, Configuration);

            using var response = await Server.Handler.HandleAsync(request).ConfigureAwait(false) ?? throw new InvalidOperationException("The root request handler did not return a response");
            
            var success = await responseHandler.Handle(request, response, keepAlive);

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
