using System.Text;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Websockets.Protocol;

namespace GenHTTP.Modules.Websockets.Provider;

public class WebsocketConnection(IRequest request, Stream stream) : IReactiveConnection, IImperativeConnection
{

    public IRequest Request => request;

    public async ValueTask WriteAsync(string payload, FrameType opcode = FrameType.Text, bool fin = true, CancellationToken token = default)
    {
        await WriteAsync(Encoding.UTF8.GetBytes(payload), opcode, fin, token: token);
    }

    public async ValueTask WriteAsync(ReadOnlyMemory<byte> payload, FrameType opcode = FrameType.Text, bool fin = true, CancellationToken token = default)
    {
        using var frameOwner = Frame.Encode(payload, opcode: (byte)opcode, fin);
        var frameMemory = frameOwner.Memory;

        // Send the frame to the WebSocket client
        await stream.WriteAsync(frameMemory, token);
        await stream.FlushAsync(token);
    }

    public async ValueTask PingAsync(CancellationToken token = default)
    {
        using var frameOwner = Frame.EncodePing();
        var frameMemory = frameOwner.Memory;

        // Send the frame to the WebSocket client
        await stream.WriteAsync(frameMemory, token);
        await stream.FlushAsync(token);
    }

    public async ValueTask PongAsync(ReadOnlyMemory<byte> payload, CancellationToken token = default)
    {
        using var frameOwner = Frame.EncodePong(payload);
        var frameMemory = frameOwner.Memory;

        // Send the frame to the WebSocket client
        await stream.WriteAsync(frameMemory, token);
        await stream.FlushAsync(token);
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
        await stream.WriteAsync(frameMemory, token);
        await stream.FlushAsync(token);
    }

    public async ValueTask<WebsocketFrame> ReadAsync(Memory<byte> buffer, CancellationToken token = default)
    {
        var receivedBytes = await stream.ReadAsync(buffer, token);

        if (receivedBytes == 0)
        {
            return new WebsocketFrame(ReadOnlyMemory<byte>.Empty, FrameType.Close);
        }

        return Frame.Decode(buffer, receivedBytes);
    }

}
