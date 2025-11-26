namespace GenHTTP.Api.Protocol;

public enum ConnectionHandling
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
    /// The server will instruct the client to upgrade to another protocol and close
    /// the connection after the response content has been sent.
    /// </summary>
    Upgrade,

    /// <summary>
    /// The connection is surrendered to the user logic and the server will not attempt
    /// to close or change it in any way.
    /// </summary>
    [Obsolete("Required to allow Fleck to write their own response. Will be removed in GenHTTP 11.")]
    UpgradeAndSurrender

}
