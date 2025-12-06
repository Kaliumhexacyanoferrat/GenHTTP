using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Websockets.Protocol;

namespace GenHTTP.Modules.Websockets;

public interface ISocketConnection
{

    IRequest Request { get; }

    ValueTask WriteAsync(string payload, FrameType opcode = FrameType.Text, bool fin = true, CancellationToken token = default);

    ValueTask WriteAsync(ReadOnlyMemory<byte> payload, FrameType opcode = FrameType.Text, bool fin = true, CancellationToken token = default);

    ValueTask PingAsync(CancellationToken token = default);

    ValueTask PongAsync(ReadOnlyMemory<byte> payload, CancellationToken token = default);

    ValueTask PongAsync(string payload, CancellationToken token = default);

    ValueTask PongAsync(CancellationToken token = default);

    ValueTask CloseAsync(string? reason = null, ushort statusCode = 1000, CancellationToken token = default);

}
