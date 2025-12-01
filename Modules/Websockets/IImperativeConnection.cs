using GenHTTP.Modules.Websockets.Protocol;

namespace GenHTTP.Modules.Websockets;

public interface IImperativeConnection : ISocketConnection
{

    ValueTask<WebsocketFrame> ReadAsync(Memory<byte> buffer, CancellationToken token = default);
    
}
