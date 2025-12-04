using System.Buffers;
using System.IO.Pipelines;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.ReverseProxy.Utils;

namespace GenHTTP.Modules.ReverseProxy.WebsocketTunnel;

public class WebsocketTunnelContent : IResponseContent
{
    public ulong? Length => null!;

    private readonly IDuplexPipe _upstreamPipe;
    private readonly RawWebsocketConnection _rawWebsocketConnection;

    public WebsocketTunnelContent(RawWebsocketConnection rawWebsocketConnection)
    {
        _rawWebsocketConnection = rawWebsocketConnection;
        _upstreamPipe = _rawWebsocketConnection.Pipe!;
    }

    public ValueTask<ulong?> CalculateChecksumAsync() => ValueTask.FromResult<ulong?>(null);

    public async ValueTask WriteAsync(Stream downstreamStream, uint bufferSize)
    {
        // Manage the tunnel lifetime and logic

        const int _rxMaxBufferSize = 4096 * 4;
        const int _txMaxBufferSize = 4096 * 4;
        
        var downstreamPipe = new DuplexPipe(PipeReader.Create(downstreamStream, 
                new StreamPipeReaderOptions(
                    MemoryPool<byte>.Shared, 
                    leaveOpen: true,
                    bufferSize: _rxMaxBufferSize, 
                    minimumReadSize: Math.Min( _rxMaxBufferSize / 4 , 1024 ))),
            PipeWriter.Create(downstreamStream, 
                new StreamPipeWriterOptions(
                    MemoryPool<byte>.Shared, 
                    leaveOpen: true,
                    minimumBufferSize: _txMaxBufferSize)));

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
    private static readonly Func<PipeReader, PipeWriter, Task> UpstreamHandler = async (upstreamReader, downstreamWriter) =>
    {
        // Read from upstream and write to downstream
        
        await upstreamReader.CopyToAsync(downstreamWriter);
        
    };

    private static readonly Func<PipeWriter, PipeReader, Task> DownstreamHandler = async (upstreamWriter, downstreamReader) =>
    {
        // Read from downstream and write to upstream
        
        await downstreamReader.CopyToAsync(upstreamWriter);
    };
}