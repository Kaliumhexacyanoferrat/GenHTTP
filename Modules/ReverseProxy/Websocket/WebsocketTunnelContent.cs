using System.Buffers;
using System.IO.Pipelines;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.ReverseProxy.Websocket;

public sealed class WebsocketTunnelContent : IResponseContent
{
    private readonly IDuplexPipe _upstreamPipe;
 
    private readonly RawWebsocketConnection _rawWebsocketConnection;

    #region Get-/Setters
    
    public ulong? Length => null!;
    
    #endregion
    
    #region Initialization
    
    public WebsocketTunnelContent(RawWebsocketConnection rawWebsocketConnection)
    {
        _rawWebsocketConnection = rawWebsocketConnection;
        _upstreamPipe = _rawWebsocketConnection.Pipe!;
    }
    
    #endregion
    
    #region Functionality

    public ValueTask<ulong?> CalculateChecksumAsync() => ValueTask.FromResult<ulong?>(null);

    public async ValueTask WriteAsync(Stream target, uint bufferSize)
    {
        await target.FlushAsync();

        const int rxMaxBufferSize = 4096 * 4;
        const int txMaxBufferSize = 4096 * 4;
        
        var downstreamPipe = new DuplexPipe(PipeReader.Create(target, 
                new StreamPipeReaderOptions(
                    MemoryPool<byte>.Shared, 
                    leaveOpen: true,
                    bufferSize: rxMaxBufferSize, 
                    minimumReadSize: Math.Min( rxMaxBufferSize / 4 , 1024 ))),
            PipeWriter.Create(target, 
                new StreamPipeWriterOptions(
                    MemoryPool<byte>.Shared, 
                    leaveOpen: true,
                    minimumBufferSize: txMaxBufferSize)));

        var upstreamHandlerTask =  UpstreamHandler(_upstreamPipe.Input, downstreamPipe.Output);
        var downstreamHandlerTask = DownstreamHandler(_upstreamPipe.Output, downstreamPipe.Input);
           
        try
        {
            // If either direction throws, Task.WhenAll will throw
            await Task.WhenAll(upstreamHandlerTask, downstreamHandlerTask);
        }
        finally
        {
            // Always run, even if Task.WhenAll threw

            // Complete downstream pipe (both ends)
            await downstreamPipe.Input.CompleteAsync();
            await downstreamPipe.Output.CompleteAsync();

            // Dispose websocket connection – this will complete the upstream pipe
            await _rawWebsocketConnection.DisposeAsync();
        }
    }
    
    // Pure delegates
    private static readonly Func<PipeReader, PipeWriter, Task> UpstreamHandler = 
        (upstreamReader, downstreamWriter) => upstreamReader.CopyToAsync(downstreamWriter);

    private static readonly Func<PipeWriter, PipeReader, Task> DownstreamHandler = 
        (upstreamWriter, downstreamReader) => downstreamReader.CopyToAsync(upstreamWriter);
    
    #endregion
    
}