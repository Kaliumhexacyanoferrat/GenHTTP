namespace GenHTTP.Api.Protocol;

public enum Connection
{

    /// <summary>
    /// The server will keep the connection open and wait for another request (default).
    /// </summary>
    KeepAlive,

    /// <summary>
    /// The server will close the connection after the response has been sent.
    /// </summary>
    Close,

    /// <summary>
    /// The server will instruct the client to upgrade to another protocol by sending
    /// the configured response and will close the connection after the response content
    /// has been sent.
    /// </summary>
    Upgrade

}
