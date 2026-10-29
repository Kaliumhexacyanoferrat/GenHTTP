namespace GenHTTP.Engine.Shared.Types.Body;

/// <summary>
/// Where the connection stands with regard to the body of the
/// current request.
/// </summary>
internal enum BodyState
{

    /// <summary>
    /// The reader is still positioned at the start of the request,
    /// so the header can be accessed.
    /// </summary>
    Pending,

    /// <summary>
    /// The reader has moved past the header and the body (if any)
    /// can be consumed.
    /// </summary>
    Opened,

    /// <summary>
    /// The connection has been handed over to another protocol.
    /// </summary>
    Upgraded

}
