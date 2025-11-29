using System.Text;
using GenHTTP.Modules.Straculo.Protocol;

namespace GenHTTP.Modules.Straculo.Utils;

/// <summary>
/// Small wrapper to improve the API
/// </summary>
public class WebsocketStream
{
    private readonly Stream _stream;
    
    public WebsocketStream(Stream stream)
    {
        _stream = stream;
    }
    
    public async ValueTask<WebsocketFrame> ReadAsync(Memory<byte> buffer, CancellationToken token = default)
    {
        var receivedBytes = await _stream.ReadAsync(buffer, token);

        if (receivedBytes == 0)
        {
            return new WebsocketFrame(ReadOnlyMemory<byte>.Empty, FrameType.Close);
        }

        return Frame.Decode(buffer, receivedBytes);
    }
    
    public async ValueTask WriteAsync(
        string payload, 
        FrameType opcode = FrameType.Text,
        bool fin = true,
        CancellationToken token = default)
    {
        await WriteAsync(Encoding.UTF8.GetBytes(payload), opcode, fin, token: token);
    }
    
    public async ValueTask WriteAsync(
        ReadOnlyMemory<byte> payload, 
        FrameType opcode = FrameType.Text,
        bool fin = true,
        CancellationToken token = default)
    {
        using var frameOwner = Frame.Encode(payload, opcode: (byte)opcode, fin);
        var frameMemory = frameOwner.Memory;

        // Send the frame to the WebSocket client
        await _stream.WriteAsync(frameMemory, token);
        await _stream.FlushAsync(token);
    }
}