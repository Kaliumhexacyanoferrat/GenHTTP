using GenHTTP.Modules.Websockets.Protocol;

namespace GenHTTP.Modules.Websockets;

/// <summary>
/// A handler that can be passed to the reactive web socket
/// implementation to handle incoming websocket frames.
/// </summary>
public interface IReactiveHandler
{

    /// <summary>
    /// Invoked when a new client has connected.
    /// </summary>
    /// <param name="connection">The connection to the client</param>
    ValueTask OnConnected(IReactiveConnection connection) => ValueTask.CompletedTask;

    /// <summary>
    /// Invoked when a message frame has been received.
    /// </summary>
    /// <param name="connection">The connection to the client</param>
    /// <param name="message">The incoming message frame</param>
    ValueTask OnMessage(IReactiveConnection connection, WebsocketFrame message) =>  ValueTask.CompletedTask;

    /// <summary>
    /// Invoked when a binary message frame has been received.
    /// </summary>
    /// <param name="connection">The connection to the client</param>
    /// <param name="message">The incoming binary message frame</param>
    ValueTask OnBinary(IReactiveConnection connection, WebsocketFrame message) =>  ValueTask.CompletedTask;

    ValueTask OnContinue(IReactiveConnection connection, WebsocketFrame message) =>  ValueTask.CompletedTask;

    /// <summary>
    /// Invoked when a ping frame has been received.
    /// </summary>
    /// <param name="connection">The connection to the client</param>
    /// <param name="message">The incoming ping frame</param>
    ValueTask OnPing(IReactiveConnection connection, WebsocketFrame message) => connection.PongAsync(message.Data);

    /// <summary>
    /// Invoked when a pong frame has been received.
    /// </summary>
    /// <param name="connection">The connection to the client</param>
    /// <param name="message">The incoming pong frame</param>
    ValueTask OnPong(IReactiveConnection connection, WebsocketFrame message) =>  ValueTask.CompletedTask;

    /// <summary>
    /// Invoked when the client requests to close the connection.
    /// </summary>
    /// <param name="connection">The connection to the client</param>
    /// <param name="message">The frame sent by the client</param>
    ValueTask OnClose(IReactiveConnection connection, WebsocketFrame message) => connection.CloseAsync();

    /// <summary>
    /// Invoked if the server failed to read a frame from the connection.
    /// </summary>
    /// <param name="connection">The connection to the client</param>
    /// <param name="error">The error which ocurred</param>
    /// <returns>true, if the connection should be closed</returns>
    ValueTask<bool> OnError(IReactiveConnection connection, FrameError error) => ValueTask.FromResult(true);

}
