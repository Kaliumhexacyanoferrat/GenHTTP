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
    ValueTask<IWebsocketFrame> ReadFrameAsync(CancellationToken token = default);

    /// <summary>
    /// Attempts to decode the next complete websocket frame from already-buffered pipe data
    /// without performing an asynchronous read. Use this to drain multiple frames that
    /// arrived in a single TCP segment before falling back to <see cref="ReadFrameAsync"/>.
    /// </summary>
    /// <remarks>
    /// Returns individual RFC 6455 frame segments; continuation-frame assembly is not performed.
    /// </remarks>
    /// <param name="frame">The decoded frame when the method returns true.</param>
    /// <returns>true if a complete frame was available; false if more data is needed.</returns>
    bool TryReadFrame(out IWebsocketFrame frame);

}
