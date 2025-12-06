using GenHTTP.Modules.Websockets.Protocol;

namespace GenHTTP.Modules.Websockets;

public interface IImperativeConnection : ISocketConnection
{

    ValueTask<WebsocketFrame> ReadFrameAsync(CancellationToken token = default);
    
}
