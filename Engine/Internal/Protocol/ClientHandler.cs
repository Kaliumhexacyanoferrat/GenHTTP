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
    private static readonly TimeSpan InitialReadTimeout = TimeSpan.FromSeconds(10);

    private static readonly TimeSpan KeepAliveTimeout = TimeSpan.FromSeconds(60);

    private IServer? _server;

    private IEndPoint? _endPoint;

    private NetworkConfiguration? _networkConfiguration;

    private Socket? _connection;

    private X509Certificate? _clientCertificate;

    private Stream? _stream;

    private PipeReader? _reader;
    
    private PipeWriter? _writer;

    private Request? _request;

    private ResponseHandler? _responseHandler;
    
    #region Get-/Setter

    internal IServer Server => _server ?? throw new InvalidOperationException("Handler has not been initialized");

    internal IEndPoint EndPoint => _endPoint ?? throw new InvalidOperationException("Handler has not been initialized");

    internal NetworkConfiguration Configuration => _networkConfiguration ?? throw new InvalidOperationException("Handler has not been initialized");

    internal Socket Connection => _connection ?? throw new InvalidOperationException("Handler has not been initialized");

    internal X509Certificate? ClientCertificate => _clientCertificate;

    internal Stream Stream => _stream ?? throw new InvalidOperationException("Handler has not been initialized");

    internal PipeWriter Writer => _writer ?? throw new InvalidOperationException("Handler has not been initialized");
    
    internal PipeReader Reader => _reader ?? throw new InvalidOperationException("Handler has not been initialized");

    private Request Request => _request ?? throw new InvalidOperationException("Handler has not been initialized");
    
    private ResponseHandler ResponseHandler => _responseHandler ?? throw new InvalidOperationException("Handler has not been initialized");

    #endregion

    #region Initialization

    internal void Apply(Socket socket, Stream stream, PipeReader reader, Request request, X509Certificate? clientCertificate, IServer server, IEndPoint endPoint, NetworkConfiguration config)
    {
        _server = server;
        _endPoint = endPoint;

        _connection = socket;
        _clientCertificate = clientCertificate;

        _networkConfiguration = config;

        _stream = stream;

        _reader = reader;
        _writer = PipeWriter.Create(stream);

        _request = request;

        _responseHandler = new ResponseHandler(Server, socket, Writer, stream, Configuration);
    }

    #endregion

    #region Functionality

    internal async ValueTask Run()
    {
        try
        {
            await HandlePipe(Reader).ConfigureAwait(false);
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
        var cts = new CancellationTokenSource(InitialReadTimeout);

        var request = Request;
        var into = request.Source;

        try
        {
            while (true)
            {
                ReadResult result;

                try
                {
                    result = await reader.ReadAsync(cts.Token);
                }
                catch (OperationCanceledException)
                {
                    return;
                }

                var buffer = result.Buffer;

                var handledRequest = false;

                while (TryParseRequest(ref buffer, into))
                {
                    request.Apply();

                    var status = await HandleRequest(request);

                    if (status is Api.Protocol.Connection.Close)
                    {
                        return;
                    }

                    request.Reset();
                    
                    handledRequest = true;
                }

                if (!handledRequest) break;

                await Writer.FlushAsync();

                reader.AdvanceTo(buffer.Start, buffer.End);

                if (result.IsCompleted) break;

                if (!cts.TryReset())
                {
                    cts.Dispose();
                    cts = new CancellationTokenSource();
                }

                cts.CancelAfter(KeepAliveTimeout);
            }
        }
        catch (HttpParseException pe)
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
            cts.Dispose();

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

        var
            keepAliveRequested =
                true; // request["Connection"]?.Equals("Keep-Alive", StringComparison.InvariantCultureIgnoreCase) ?? request.ProtocolType == HttpProtocol.Http11;

        var response = await Server.Handler.HandleAsync(request) ??
                       throw new InvalidOperationException("The root request handler did not return a response");

        var closeRequested =
            false; // response.Connection is Api.Protocol.Connection.Close or Api.Protocol.Connection.Upgrade;

        var active =
            ResponseHandler.Handle(request, response, HttpProtocol.Http11, keepAliveRequested && !closeRequested);

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