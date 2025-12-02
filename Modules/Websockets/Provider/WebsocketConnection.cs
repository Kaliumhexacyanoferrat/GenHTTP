using System.Buffers;
using System.IO.Pipelines;
using System.Text;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Websockets.Protocol;

namespace GenHTTP.Modules.Websockets.Provider;

public class WebsocketConnection : IReactiveConnection, IImperativeConnection
{
    private readonly Stream _stream;
    private readonly PipeReader _pipeReader;
    private readonly int _rxMaxBufferSize;
    
    public WebsocketConnection(IRequest request, Stream stream, int rxMaxBufferSize = 8192)
    {
        Request = request;
        _stream = stream;
        _rxMaxBufferSize =  rxMaxBufferSize;
        _pipeReader = PipeReader.Create(stream, 
            new StreamPipeReaderOptions(
                MemoryPool<byte>.Shared, 
                leaveOpen: false,
                bufferSize: rxMaxBufferSize, 
                minimumReadSize: Math.Min( rxMaxBufferSize / 4 , 1024 )));
    }

    public IRequest Request { get; }

    public async ValueTask WriteAsync(string payload, FrameType opcode = FrameType.Text, bool fin = true, CancellationToken token = default)
    {
        await WriteAsync(Encoding.UTF8.GetBytes(payload), opcode, fin, token: token);
    }

    public async ValueTask WriteAsync(ReadOnlyMemory<byte> payload, FrameType opcode = FrameType.Text, bool fin = true, CancellationToken token = default)
    {
        using var frameOwner = Frame.Encode(payload, opcode: (byte)opcode, fin);
        var frameMemory = frameOwner.Memory;

        // Send the frame to the WebSocket client
        await _stream.WriteAsync(frameMemory, token);
        await _stream.FlushAsync(token);
    }

    public async ValueTask PingAsync(CancellationToken token = default)
    {
        using var frameOwner = Frame.EncodePing();
        var frameMemory = frameOwner.Memory;

        // Send the frame to the WebSocket client
        await _stream.WriteAsync(frameMemory, token);
        await _stream.FlushAsync(token);
    }

    public async ValueTask PongAsync(ReadOnlyMemory<byte> payload, CancellationToken token = default)
    {
        using var frameOwner = Frame.EncodePong(payload);
        var frameMemory = frameOwner.Memory;

        // Send the frame to the WebSocket client
        await _stream.WriteAsync(frameMemory, token);
        await _stream.FlushAsync(token);
    }

    public async ValueTask PongAsync(string payload, CancellationToken token = default)
    {
        await PongAsync(Encoding.UTF8.GetBytes(payload), token);
    }

    public async ValueTask PongAsync(CancellationToken token = default)
    {
        await PongAsync(ReadOnlyMemory<byte>.Empty, token);
    }

    public async ValueTask CloseAsync(string? reason = null, ushort statusCode = 1000, CancellationToken token = default)
    {
        using var frameOwner = Frame.EncodeClose(reason, statusCode);
        var frameMemory = frameOwner.Memory;

        // Send the frame to the WebSocket client
        await _stream.WriteAsync(frameMemory, token);
        await _stream.FlushAsync(token);
    }
    
    /// <summary>
    /// Wait for a frame to be acquired.
    /// </summary>
    public async ValueTask<WebsocketFrame> ReadFrameAsync(CancellationToken token = default)
    {
        while (true)
        {
            var result = await _pipeReader.ReadAsync(token);
            var buffer = result.Buffer;

            if (result.IsCanceled)
            {
                return new WebsocketFrame(
                    ReadOnlyMemory<byte>.Empty, 
                    FrameType.Error, 
                    FrameError: new FrameError("Read was canceled", FrameErrorType.Canceled));
            }
            
            if (buffer.Length == 0 && result.IsCompleted) // Clean EOF: no more data, reader completed
            {
                _pipeReader.AdvanceTo(buffer.Start, buffer.End);
                return new WebsocketFrame(ReadOnlyMemory<byte>.Empty, FrameType.Close);
            }

            var frame = Frame.Decode(ref result, _rxMaxBufferSize, out var consumed, out var examined);

            _pipeReader.AdvanceTo(consumed, examined);

            // Need more data for a complete frame
            if (frame.Type == FrameType.Error &&
                frame.FrameError!.ErrorType == FrameErrorType.Incomplete)
            {
                if (result.IsCompleted)
                {
                    return new WebsocketFrame(
                        ReadOnlyMemory<byte>.Empty, 
                        FrameType.Error, 
                        FrameError: new FrameError("Unexpected end of stream while reading WebSocket frame.", FrameErrorType.IncompleteForever));
                }
                
                continue;  // read more data
            }

            // Any other frame (including other errors)
            return frame;
        }
    }
}
