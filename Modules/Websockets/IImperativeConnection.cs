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
    
    /// <summary>
    /// Advances consumed and examined data, memory portion is no longer guaranteed to be valid.
    /// Should never be used if segmented frames are being handled automatically.
    /// </summary>
    void Consume();

    /// <summary>
    /// Advances examined data, marks the pipe reader we are ready for more data.
    /// </summary>
    void Examine();

    void Advance();
}
