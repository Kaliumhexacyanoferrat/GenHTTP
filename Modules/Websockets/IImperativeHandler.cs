namespace GenHTTP.Modules.Websockets;

/// <summary>
/// A handler that can be passed to the imperative web socket
/// implementation to implement a message pump.
/// </summary>
public interface IImperativeHandler
{

    /// <summary>
    /// Invoked by the handler as soon as a connection to
    /// a web socket client is established. Use the connection
    /// to read incoming messages and handle them as needed.
    /// </summary>
    /// <param name="connection">The connection to read incoming web socket frames from</param>
    ValueTask HandleAsync(IImperativeConnection connection);

}
