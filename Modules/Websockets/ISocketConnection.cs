using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Websockets.Protocol;

namespace GenHTTP.Modules.Websockets;

/// <summary>
/// A connection which can be used to write websocket messages
/// to the underlying stream.
/// </summary>
public interface ISocketConnection
{

    /// <summary>
    /// The request that initially caused the web socket connection.
    /// </summary>
    IRequest Request { get; }

    /// <summary>
    /// Creates a web socket frame from the given payload and options
    /// and sends it to the client.
    /// </summary>
    /// <param name="payload">The payload to be sent</param>
    /// <param name="opcode">The kind of the frame to be sent</param>
    /// <param name="fin">false, if you would like to break down large chunks of data into several frames (last one has to be sent with fin = true)</param>
    /// <param name="token">A token which can be used to cancel the sending process</param>
    ValueTask WriteAsync(ReadOnlyMemory<byte> payload, FrameType opcode = FrameType.Text, bool fin = true, CancellationToken token = default);

    /// <summary>
    /// Sends a ping frame to the connected client.
    /// </summary>
    /// <param name="token">A token which can be used to cancel the sending process</param>
    ValueTask PingAsync(CancellationToken token = default);

    /// <summary>
    /// Creates a pong frame with the given content and sends it to
    /// the client.
    /// </summary>
    /// <param name="payload">The payload to be sent</param>
    /// <param name="token">A token which can be used to cancel the sending process</param>
    ValueTask PongAsync(ReadOnlyMemory<byte> payload, CancellationToken token = default);

    /// <summary>
    /// Creates a pong frame and sends it to the client.
    /// </summary>
    /// <param name="token">A token which can be used to cancel the sending process</param>
    ValueTask PongAsync(CancellationToken token = default);

    void Consume();

    /// <summary>
    /// Closes the web socket connection. After this method has been called,
    /// the connection can no longer be used to send frames.
    /// </summary>
    /// <param name="reason">The reason why the connection should be closed</param>
    /// <param name="statusCode">The status code to be sent to the client</param>
    /// <param name="token">A token which can be used to cancel the sending process</param>
    ValueTask CloseAsync(string? reason = null, ushort statusCode = 1000, CancellationToken token = default);

}
