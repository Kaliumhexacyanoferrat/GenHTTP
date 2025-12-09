using System.Text;
using GenHTTP.Modules.Websockets.Protocol;

namespace GenHTTP.Modules.Websockets;

public static class ConnectionTextExtensions
{

    /// <summary>
    /// Creates a web socket frame from the given payload and options
    /// and sends it to the client.
    /// </summary>
    /// <param name="payload">The payload to be sent</param>
    /// <param name="opcode">The kind of the frame to be sent</param>
    /// <param name="fin">false, if you would like to break down large chunks of data into several frames (last one has to be sent with fin = true)</param>
    /// <param name="token">A token which can be used to cancel the sending process</param>
    public static ValueTask WriteAsync(this ISocketConnection connection, string payload, FrameType opcode = FrameType.Text, bool fin = true, CancellationToken token = default)
        => connection.WriteAsync(Encoding.UTF8.GetBytes(payload), opcode, fin, token: token);

    /// <summary>
    /// Creates a pong frame with the given content and sends it to
    /// the client.
    /// </summary>
    /// <param name="payload">The payload to be sent</param>
    /// <param name="token">A token which can be used to cancel the sending process</param>
    public static ValueTask PongAsync(this ISocketConnection connection, string payload, CancellationToken token = default)
        => connection.PongAsync(Encoding.UTF8.GetBytes(payload), token);

}
