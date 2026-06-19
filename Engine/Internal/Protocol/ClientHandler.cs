using System.Buffers;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Engine.Internal.Context;
using GenHTTP.Engine.Shared.Types;
using Glyph11;
using Glyph11.Parser;
using Glyph11.Parser.UltraHardened;
using Glyph11.Protocol;
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
internal sealed class ClientHandler(ClientContext context)
{
    private static readonly TimeSpan InitialReadTimeout = TimeSpan.FromSeconds(10);

    private static readonly TimeSpan KeepAliveTimeout = TimeSpan.FromSeconds(60);

    private static readonly ReadOnlyMemory<byte> KeepAliveValue = "Keep-Alive"u8.ToArray();

    private static readonly ParserLimits Limits = ParserLimits.Default;

    private CancellationTokenSource _cts = new();

    #region Functionality

    internal async ValueTask RunAsync()
    {
        var connection = context.Connection;

        try
        {
            await HandlePipeAsync(context.Reader).ConfigureAwait(false);
        }
        catch
        {
            // todo: logging
        }
        finally
        {
            try
            {
                await context.Stream.DisposeAsync();
            }
            catch
            {
                // todo: logging
            }

            try
            {
                connection.Shutdown(SocketShutdown.Both);

                await connection.DisconnectAsync(false);

                connection.Close();

                connection.Dispose();
            }
            catch
            {
                // todo: logging
            }
        }
    }

    private async ValueTask HandlePipeAsync(PipeReader reader)
    {
        ResetCts(InitialReadTimeout);

        var request = context.Request;
        var into = request.Source;

        ReadResult readResult = default;
        var dataRemaining = false;

        try
        {
            while (context.Server.Running)
            {
                if (!dataRemaining)
                {
                    try
                    {
                        readResult = await reader.ReadAsync(_cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                    catch (IOException e) when (e.InnerException is SocketException { SocketErrorCode: SocketError.ConnectionReset or SocketError.ConnectionAborted })
                    {
                        return;
                    }
                    catch (IOException e) when (e.Message.Contains("Broken pipe", StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }
                }

                dataRemaining = false;

                var buffer = readResult.Buffer;

                if (!TryParseRequest(ref buffer, into))
                {
                    reader.AdvanceTo(readResult.Buffer.Start, readResult.Buffer.End);
                    if (readResult.IsCompleted) break;
                    continue;
                }

                request.Apply(context.Server, context.EndPoint, reader, buffer.Start, context.Connection.GetAddress(), context.ClientCertificate);

                var status = await HandleRequestAsync(request);

                if (!context.Connection.Connected)
                {
                    return;
                }

                if (status is Connection.Upgrade)
                {
                    // content handler is responsible for flushing, avoid duplicate flushes
                    return;
                }
                else if (status is Connection.Close)
                {
                    await context.Writer.FlushAsync();
                    return;
                }

                await request.DrainAsync();

                request.Reset();

                if (readResult.IsCompleted) break;

                if (reader.TryRead(out var next)) // pipeline mode (more data available)
                {
                    readResult = next;
                    dataRemaining = true;
                }
                else
                {
                    ResetCts(KeepAliveTimeout);
                    await context.Writer.FlushAsync();
                }
            }
        }
        catch (HttpParseException pe)
        {
            // client did something wrong
            await SendErrorAsync(pe, (ResponseStatus)pe.StatusCode);
            throw;
        }
        catch (Exception e)
        {
            // we did something wrong
            await SendErrorAsync(e, ResponseStatus.InternalServerError);
            throw;
        }
        finally
        {
            await reader.CompleteAsync();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryParseRequest(ref ReadOnlySequence<byte> buffer, BinaryRequest into)
    {
        if (!UltraHardenedParser.TryExtractFullHeaderValidated(ref buffer, into, Limits, out var bytesRead))
        {
            return false;
        }

        buffer = buffer.Slice(bytesRead + 1);
        return true;
    }

    internal async ValueTask<Connection> HandleRequestAsync(Request request)
    {
        var header = request.Header;

        if (!header.Headers.ContainsKey(KnownHeaders.Host))
        {
            throw new ProviderException(ResponseStatus.BadRequest, "Host header is missing from the request");
        }

        var headRequest = header.Method == RequestMethod.Head;

        var connectionHeader = header.Headers.GetEntry(KnownHeaders.Connection);

        var keepAliveRequested = connectionHeader?.Bytes.Span.SequenceEqual(KeepAliveValue.Span) ?? (header.Protocol == HttpProtocol.Http11);

        var response = await context.Server.Handler.HandleAsync(request) ?? throw new InvalidOperationException("The root request handler did not return a response");

        var upgradeResponse = response.Mode is Connection.Upgrade;

        var closeRequested = response.Mode is Connection.Close || upgradeResponse;

        var active = await context.ResponseHandler.HandleAsync(request, response, HttpProtocol.Http11, keepAliveRequested && !closeRequested, headRequest);

        if (upgradeResponse)
        {
            return Connection.Upgrade;
        }

        return (active && keepAliveRequested && !closeRequested) ? Connection.KeepAlive : Connection.Close;
    }

    private async ValueTask SendErrorAsync(Exception e, ResponseStatus status)
    {
        try
        {
            var message = context.Server.Development ? e.ToString() : e.Message;

            var response = new ResponseBuilder()
                           .Status(status)
                           .Connection(Connection.Close)
                           .Content(new StringContent(message))
                           .Build();

            await context.ResponseHandler.HandleAsync(null, response, HttpProtocol.Http10, false, false);

            await context.Writer.FlushAsync();
        }
        catch
        {
            /* no recovery here */
        }
    }

    private void ResetCts(TimeSpan timeout)
    {
        if (!_cts.TryReset())
        {
            _cts.Dispose();
            _cts = new CancellationTokenSource();
        }

        _cts.CancelAfter(timeout);
    }

    #endregion

}
