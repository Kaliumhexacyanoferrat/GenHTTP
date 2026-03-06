using System.Buffers;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Engine.Internal.Context;
using GenHTTP.Engine.Internal.Parser;
using GenHTTP.Engine.Internal.Utilities;
using GenHTTP.Engine.Shared.Infrastructure;
using GenHTTP.Engine.Shared.Types;
using Glyph11;
using Glyph11.Parser.FlexibleParser;
using Glyph11.Protocol;
using Glyph11.Validation;
using Microsoft.Extensions.ObjectPool;
using StringContent = GenHTTP.Modules.IO.Strings.StringContent;

namespace GenHTTP.Engine.Internal.Protocol;

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
    private static readonly StreamPipeReaderOptions ReaderOptions =
        new(MemoryPool<byte>.Shared, leaveOpen: true, bufferSize: 65536);

    private static readonly DefaultObjectPool<ClientContext> ContextPool = new(new ClientContextPolicy(), 65536);

    #region Get-/Setter

    internal IServer Server { get; }

    internal IEndPoint EndPoint { get; }

    internal NetworkConfiguration Configuration { get; }

    internal Socket Connection { get; }

    internal X509Certificate? ClientCertificate { get; set; }

    internal Stream Stream { get; }

    internal PipeWriter Writer { get; }

    private ResponseHandler ResponseHandler { get; }

    #endregion

    #region Initialization

    internal ClientHandler(Socket socket, Stream stream, X509Certificate? clientCertificate, IServer server,
        IEndPoint endPoint, NetworkConfiguration config)
    {
        Server = server;
        EndPoint = endPoint;

        Connection = socket;
        ClientCertificate = clientCertificate;

        Configuration = config;

        Stream = stream;
        Writer = PipeWriter.Create(stream);

        ResponseHandler = new ResponseHandler(Server, socket, Writer, stream, Configuration);
    }

    #endregion

    #region Functionality

    internal async ValueTask Run()
    {
        try
        {
            await HandlePipe(PipeReader.Create(Stream, ReaderOptions)).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Server.Companion?.OnServerError(ServerErrorScope.ClientConnection, Connection.GetAddress(), e);
        }
        finally
        {
            try
            {
                await Stream.DisposeAsync();
            }
            catch (Exception e)
            {
                Server.Companion?.OnServerError(ServerErrorScope.ClientConnection, Connection.GetAddress(), e);
            }

            try
            {
                Connection.Shutdown(SocketShutdown.Both);
                await Connection.DisconnectAsync(false);
                Connection.Close();

                Connection.Dispose();
            }
            catch (Exception e)
            {
                Server.Companion?.OnServerError(ServerErrorScope.ClientConnection, Connection.GetAddress(), e);
            }
        }
    }

    private async ValueTask HandlePipe(PipeReader reader)
    {
        var context = ContextPool.Get();
        
        try
        {
            var request = context.Request;
            
            var into = request.Source;
            
            while (true)
            {
                var result = await reader.ReadAsync();
                
                var buffer = result.Buffer;

                while (TryParseRequest(ref buffer, into))
                {
                    request.Apply();
                    
                    var status = await HandleRequest(context.Request);
                    
                    if (status is Api.Protocol.Connection.Close)
                    {
                        return;
                    }
                    
                    request.Reset();
                }

                await Writer.FlushAsync();

                reader.AdvanceTo(buffer.Start, buffer.End);

                if (result.IsCompleted) break;
            }
        }
        catch (ProtocolException pe)
        {
            // client did something wrong
            SendError(pe, 400);
            throw;
        }
        catch (Exception e)
        {
            // we did something wrong
            SendError(e, 500);
            throw;
        }
        finally
        {
            ContextPool.Return(context);

            await reader.CompleteAsync();
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryParseRequest(ref ReadOnlySequence<byte> buffer, BinaryRequest into)
    {
        if (!FlexibleParser.TryExtractFullHeader(ref buffer, into, out var bytesRead))
        {
            return false;
        }

        buffer = buffer.Slice(bytesRead);
        return true;
    }

    private async ValueTask<Connection> HandleRequest(Request request)
    {
        // request.SetConnection(Server, EndPoint, Connection.GetAddress(), ClientCertificate);

        var keepAliveRequested = true; // request["Connection"]?.Equals("Keep-Alive", StringComparison.InvariantCultureIgnoreCase) ?? request.ProtocolType == HttpProtocol.Http11;

        var response = await Server.Handler.HandleAsync(request) ?? throw new InvalidOperationException("The root request handler did not return a response");

        var closeRequested = false; // response.Connection is Api.Protocol.Connection.Close or Api.Protocol.Connection.Upgrade;

        var active = ResponseHandler.Handle(request, response, HttpProtocol.Http11, keepAliveRequested && !closeRequested);

        return (active && keepAliveRequested && !closeRequested)
            ? Api.Protocol.Connection.KeepAlive
            : Api.Protocol.Connection.Close;
    }

    private void SendError(Exception e, int status)
    {
        try
        {
            var message = Server.Development ? e.ToString() : e.Message;

            // todo status code mapping


            var response = new ResponseBuilder().Raw()
                .Status(status, status == 500 ? "Internal Server Error"u8.ToArray() : "Bad Request"u8.ToArray())
                .Content(new StringContent(message))
                .Build();

            ResponseHandler.Handle(null, response, HttpProtocol.Http10, false);
        }
        catch
        {
            /* no recovery here */
        }
    }

    #endregion
    
}