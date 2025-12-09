namespace GenHTTP.Modules.Websockets;

/// <summary>
/// A connection used by the reactive handler to
/// send frames to the connected client.
/// </summary>
public interface IReactiveConnection : ISocketConnection;
