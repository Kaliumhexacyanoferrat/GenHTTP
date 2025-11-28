using System.Text;
using GenHTTP.Modules.Straculo.Protocol;

namespace GenHTTP.Modules.Straculo.Reactive;

/// <summary>
/// Small wrapper to improve the API
/// </summary>
public class ReactiveWebsocketStream
{
    private readonly Stream _stream;
    
    public ReactiveWebsocketStream(Stream stream)
    {
        _stream = stream;
    }
    
    public async ValueTask WriteAsync(
        ReadOnlyMemory<byte> payload, 
        FrameType opcode = FrameType.Text,
        CancellationToken token = default)
    {
        using var frameOwner = Frame.Build(payload, opcode: (byte)opcode);
        var frameMemory = frameOwner.Memory;

        // Send the frame to the WebSocket client
        await _stream.WriteAsync(frameMemory, token);
        await _stream.FlushAsync(token);
    }
    
    public async ValueTask WriteAsync(
        string payload, 
        FrameType opcode = FrameType.Text,
        CancellationToken token = default)
    {
        using var frameOwner = Frame.Build(Encoding.UTF8.GetBytes(payload), opcode: (byte)opcode);
        var frameMemory = frameOwner.Memory;

        // Send the frame to the WebSocket client
        await _stream.WriteAsync(frameMemory, token);
        await _stream.FlushAsync(token);
    }
}