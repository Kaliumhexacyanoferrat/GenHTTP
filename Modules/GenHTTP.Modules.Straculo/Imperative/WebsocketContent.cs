using System.Text;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Straculo.Protocol;

namespace GenHTTP.Modules.Straculo.Imperative;

public abstract class WebsocketContent : IResponseContent
{
    public ulong? Length { get; } = null!;

    public ValueTask<ulong?> CalculateChecksumAsync() => new((ulong)GetHashCode());

    public abstract ValueTask WriteAsync(Stream target, uint bufferSize);
    
    protected static async ValueTask<WebsocketFrame> ReadAsync(Stream target, Memory<byte> buffer, CancellationToken token = default)
    {
        var receivedBytes = await target.ReadAsync(buffer, token);

        if (receivedBytes == 0)
        {
            return new WebsocketFrame(ReadOnlyMemory<byte>.Empty, FrameType.Close);
        }

        return Frame.Decode(buffer, receivedBytes);
    }

    protected static async ValueTask WriteAsync(
        Stream target, 
        ReadOnlyMemory<byte> payload, 
        FrameType opcode = FrameType.Text,
        bool fin = true,
        CancellationToken token = default)
    {
        using var frameOwner = Frame.Encode(payload, opcode: (byte)opcode);
        var frameMemory = frameOwner.Memory;

        // Send the frame to the WebSocket client
        await target.WriteAsync(frameMemory, token);
        await target.FlushAsync(token);
    }
    
    protected static async ValueTask WriteAsync(
        Stream target, 
        string payload, 
        FrameType opcode = FrameType.Text,
        bool fin = true,
        CancellationToken token = default)
    {
        using var frameOwner = Frame.Encode(Encoding.UTF8.GetBytes(payload), opcode: (byte)opcode);
        var frameMemory = frameOwner.Memory;

        // Send the frame to the WebSocket client
        await target.WriteAsync(frameMemory, token);
        await target.FlushAsync(token);
    }
}