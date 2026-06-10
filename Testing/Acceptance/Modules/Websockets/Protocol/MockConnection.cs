using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Conversion;
using GenHTTP.Modules.Conversion.Serializers.Json;
using GenHTTP.Modules.Websockets;
using GenHTTP.Modules.Websockets.Protocol;
using GenHTTP.Modules.Websockets.Provider;

namespace GenHTTP.Testing.Acceptance.Modules.Websockets.Protocol;

public class MockConnection : ISocketConnection
{

    public IRequest Request => throw new NotImplementedException();

    public ConnectionSettings Settings => new ConnectionSettings(Formatting.Default().Build(), new JsonFormat(), 128, true, true);

    public ValueTask WriteAsync(ReadOnlyMemory<byte> payload, FrameType opcode = FrameType.Text, bool fin = true, bool flush = true, CancellationToken token = default) => throw new NotImplementedException();

    public ValueTask PingAsync(bool flush = true, CancellationToken token = default) => throw new NotImplementedException();

    public ValueTask PongAsync(ReadOnlyMemory<byte> payload, bool flush = true, CancellationToken token = default) => throw new NotImplementedException();

    public ValueTask PongAsync(bool flush = true, CancellationToken token = default) => throw new NotImplementedException();

    public ValueTask CloseAsync(string? reason = null, ushort statusCode = 1000, bool flush = true, CancellationToken token = default) => throw new NotImplementedException();

    public ValueTask FlushAsync(CancellationToken token = default) => throw new NotImplementedException();

}
