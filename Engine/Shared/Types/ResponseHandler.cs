using System.Runtime.CompilerServices;
using System.Buffers;

using GenHTTP.Api.Protocol;

using GenHTTP.Engine.Shared.Types.Sinks;
using Microsoft.Extensions.Logging;

namespace GenHTTP.Engine.Shared.Types;

public sealed class ResponseHandler
{
    private readonly RegularSink _regularSink;

    private readonly ChunkedSink _chunkedSink;

    private IClientContext Context { get; }

    #region Initialization

    public ResponseHandler(IClientContext context)
    {
        Context = context;

        _regularSink = new(Context);
        _chunkedSink = new(Context);
    }

    #endregion

    #region Functionality

    public async ValueTask<bool> HandleAsync(IRequest? request, IResponse response, HttpProtocol version, bool keepAlive, bool headRequest)
    {
        try
        {
            var writer = Context.Writer;

            writer.Write(StatusLine.Get(response.Status));

            WriteHeader(response, version, keepAlive);

            writer.Write("\r\n"u8);

            if (ResponseSerializer.ShouldSendBody(request, response, headRequest))
            {
                await WriteBodyAsync(response);
            }

            return true;
        }
        catch (Exception e)
        {
            if (!ConnectionExceptions.IsGracefulDisconnect(e))
            {
                Context.Server.Logging.CreateLogger<ResponseHandler>().LogWarning(e, "Failed to write response to client");
            }

            return false;
        }
    }

    private void WriteHeader(IResponse response, HttpProtocol version, bool keepAlive)
    {
        var context = Context;

        ResponseSerializer.WriteHeader(context.Writer, response, keepAlive, ServerHeader.GetValue(context), DateHeader.GetValue(), version == HttpProtocol.Http10);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private async ValueTask WriteBodyAsync(IResponse response)
    {
        var content = response.Content;

        if (content is null)
        {
            return;
        }

        var length = content.Length;

        if (length is null && response.Mode != Connection.Upgrade)
        {
            await WriteChunked(content);
        }
        else
        {
            _regularSink.Apply();
            await content.WriteAsync(_regularSink);
        }

        if (content is IDisposable disposableContent)
        {
            disposableContent.Dispose();
        }
    }

    private async ValueTask WriteChunked(IResponseContent content)
    {
        _chunkedSink.Apply();

        await content.WriteAsync(_chunkedSink);

        _chunkedSink.Finish();
    }

    #endregion

}
