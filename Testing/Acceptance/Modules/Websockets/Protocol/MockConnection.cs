using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Websockets;
using GenHTTP.Modules.Websockets.Protocol;
using GenHTTP.Modules.Websockets.Provider;

namespace GenHTTP.Testing.Acceptance.Modules.Websockets.Protocol;

public class MockConnection : ISocketConnection
{

    public IRequest Request => throw new NotImplementedException();

    public ConnectionSettings Settings => throw new NotImplementedException();

    public ValueTask WriteAsync(ReadOnlyMemory<byte> payload, FrameType opcode = FrameType.Text, bool fin = true, CancellationToken token = default) => throw new NotImplementedException();

    public ValueTask PingAsync(CancellationToken token = default) => throw new NotImplementedException();

    public ValueTask PongAsync(ReadOnlyMemory<byte> payload, CancellationToken token = default) => throw new NotImplementedException();

    public ValueTask PongAsync(CancellationToken token = default) => throw new NotImplementedException();

    public ValueTask CloseAsync(string? reason = null, ushort statusCode = 1000, CancellationToken token = default) => throw new NotImplementedException();

}
