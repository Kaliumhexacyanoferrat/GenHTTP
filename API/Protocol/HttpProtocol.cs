namespace GenHTTP.Api.Protocol;

/// <summary>
/// The protocol version of a request.
/// </summary>
public enum HttpProtocol
{

    /// <summary>
    /// HTTP/1.0
    /// </summary>
    Http10,

    /// <summary>
    /// HTTP/1.1
    /// </summary>
    Http11,

    /// <summary>
    /// HTTP/2
    /// </summary>
    Http2,

    /// <summary>
    /// HTTP/3
    /// </summary>
    Http3

}
