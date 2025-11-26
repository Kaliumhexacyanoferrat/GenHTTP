using System.Buffers;
using System.Text;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Straculo.Protocol;

namespace GenHTTP.Modules.Straculo;

public abstract class Websocket : IResponseContent
{
    private readonly IRequest _request;

    public Websocket(IRequest request)
    {
        _request = request;
    }

    public ulong? Length { get; }

    public ValueTask<ulong?> CalculateChecksumAsync() => new((ulong)_request.GetHashCode());

    public abstract ValueTask WriteAsync(Stream target, uint bufferSize);
    
    protected async ValueTask<WebsocketFrame> ReadAsync(Stream target, Memory<byte> buffer, CancellationToken token = default)
    {
        var receivedBytes = await target.ReadAsync(buffer, token);

        if (receivedBytes == 0)
        {
            return new WebsocketFrame()
            {
                Data = ReadOnlyMemory<byte>.Empty,
                Type = FrameType.Close
            };
        }

        var decodedFrame = Frame.Decode(buffer, receivedBytes, out var frameType);
        
        return new WebsocketFrame
        {
            Data = decodedFrame,
            Type = frameType
        };
    }

    protected async ValueTask WriteAsync(
        Stream target, 
        ReadOnlyMemory<byte> payload, 
        byte opcode = 0x01, 
        CancellationToken token = default)
    {
        using var frameOwner = Frame.Build(payload, opcode: opcode);
        var frameMemory = frameOwner.Memory;

        // Send the frame to the WebSocket client
        await target.WriteAsync(frameMemory, token);
    }
}

public class WebsocketFrame
{
    public ReadOnlyMemory<byte> Data { get; init; }
    
    public FrameType Type { get; init; }
}