using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Engine.Infrastructure.Configuration;
using GenHTTP.Engine.Protocol;
using GenHTTP.Engine.Protocol.Parser;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.IO.Streaming;
using PooledAwait;
using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;

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

        private ResponseHandler ResponseHandler { get; set; }

        #endregion

        #region Initialization

        internal ClientHandler(Socket socket, Stream stream, IServer server, IEndPoint endPoint, NetworkConfiguration config)
        {
            Server = server;
            EndPoint = endPoint;

            Configuration = config;
            Connection = socket;

            Stream = stream;

            ResponseHandler = new ResponseHandler(Server, Stream, Configuration);
        }

        #endregion

        #region Functionality

        internal async PooledValueTask Run()
        {
            try
            {
                await HandlePipe(PipeReader.Create(Stream, READER_OPTIONS));
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
                    Connection.Shutdown(SocketShutdown.Both);
                    Connection.Disconnect(false);
                    Connection.Close();

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

                try
                {
                    while (Server.Running && (request = await parser.TryParseAsync(buffer)) is not null)
                    {
                        if (!await HandleRequest(request, !buffer.ReadRequired))
                        {
                            break;
                        }
                    }
                }
                catch (ProtocolException pe)
                {
                    // client did something wrong
                    await SendError(pe, ResponseStatus.BadRequest);
                    throw;
                }
                catch (Exception e)
                {
                    // we did something wrong
                    await SendError(e, ResponseStatus.InternalServerError);
                    throw;
                }
            }
            finally
            {
                await reader.CompleteAsync();
            }
        }

        private async PooledValueTask<bool> HandleRequest(RequestBuilder builder, bool dataRemaining)
        {
            var address = (Connection.RemoteEndPoint as IPEndPoint)?.Address;

            using var request = builder.Connection(Server, EndPoint, address).Build();

            KeepAlive ??= request["Connection"]?.Equals("Keep-Alive", StringComparison.InvariantCultureIgnoreCase) ?? (request.ProtocolType == HttpProtocol.Http_1_1);

            bool keepAlive = KeepAlive.Value;

            using var response = await Server.Handler.HandleAsync(request) ?? throw new InvalidOperationException("The root request handler did not return a response");

            var success = await ResponseHandler.Handle(request, response, keepAlive, dataRemaining);

            return (success && keepAlive);
        }

        private async PooledValueTask SendError(Exception e, ResponseStatus status)
        {
            try
            {
                var message = Server.Development ? e.ToString() : e.Message;

                var content = Resource.FromString(message)
                                      .Build();

                var response = new ResponseBuilder().Status(status)
                                                    .Content(new ResourceContent(content))
                                                    .Build();

                await ResponseHandler.Handle(null, response, false, false);
            }
            catch { /* no recovery here */ }
        }

        #endregion

    }

}
