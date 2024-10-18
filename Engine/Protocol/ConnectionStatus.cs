namespace GenHTTP.Engine.Protocol;

/// <summary>
/// Tells the client handler how to proceed with this client.
/// </summary>
public enum ConnectionStatus
{

    /// <summary>
    /// The connection should gracefully be closed.
    /// </summary>
    Close,

    /// <summary>
    /// The connection should be used to read another HTTP request.
    /// </summary>
    KeepAlive,

    /// <summary>
    /// The connection has been upgraded so the server should ignore
    /// the socket further on.
    /// </summary>
    Upgraded

}
