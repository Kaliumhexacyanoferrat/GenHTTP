using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Websockets;

/// <summary>
/// Represents a connected websocket client.
/// </summary>
public interface IWebsocketConnection
{

    /// <summary>
    /// The original request the websocket connection was created from.
    /// </summary>
    IRequest Request { get; }

    /// <summary>
    /// Returns true if the socket is connected and can receive messages.
    /// </summary>
    bool IsAvailable { get; }

    /// <summary>
    /// Sends a text message to the connected client.
    /// </summary>
    /// <param name="message">The message to be sent</param>
    Task Send(string message);

    /// <summary>
    /// Sends a binary message to the connected client.
    /// </summary>
    /// <param name="message">The message to be sent</param>
    Task Send(byte[] message);

    /// <summary>
    /// Sends a ping message to the connected client.
    /// </summary>
    /// <param name="message">The message to be sent</param>
    Task SendPing(byte[] message);

    /// <summary>
    /// Sends a pong message to the connected client.
    /// </summary>
    /// <param name="message">The message to be sent</param>
    Task SendPong(byte[] message);

    /// <summary>
    /// Gracefully closes the connection to the client.
    /// </summary>
    void Close();

    /// <summary>
    /// Closes the connection with the specified code.
    /// </summary>
    /// <param name="code">The code to be sent to the connected client</param>
    /// <remarks>
    /// See Flex.WebSocketStatusCodes.
    /// </remarks>
    void Close(int code);

}
