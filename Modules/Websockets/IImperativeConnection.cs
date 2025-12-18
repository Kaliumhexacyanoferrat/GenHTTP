using GenHTTP.Modules.Websockets.Protocol;

namespace GenHTTP.Modules.Websockets;

/// <summary>
/// A connection used by the imperative handler to
/// fetch incoming frames from the underlying stream.
/// </summary>
public interface IImperativeConnection : ISocketConnection
{

    /// <summary>
    /// Attempts to read the next websocket frame from the
    /// underlying connection. Blocks until a frame is received.
    /// </summary>
    /// <param name="token">A token which can be used to cancel the operation</param>
    /// <returns>The next websocket frame to be handled</returns>
    ValueTask<WebsocketFrame> ReadFrameAsync(CancellationToken token = default);

    void Advance();
}
