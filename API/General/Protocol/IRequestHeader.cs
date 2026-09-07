namespace GenHTTP.Api.Protocol;

/// <summary>
/// Represents the header of an incoming HTTP request.
/// </summary>
public interface IRequestHeader
{

    /// <summary>
    /// The protocol used by the client.
    /// </summary>
    HttpProtocol Protocol { get; }

    /// <summary>
    /// The request method (or verb).
    /// </summary>
    RequestMethod Method { get; }

    /// <summary>
    /// The requested path.
    /// </summary>
    ByteString Path { get; }

    /// <summary>
    /// The routable target, constructed from the path.
    /// </summary>
    IRequestTarget Target { get; }

    /// <summary>
    /// The query sent by the client.
    /// </summary>
    IRequestQuery Query { get; }

    /// <summary>
    /// The headers sent by the client.
    /// </summary>
    IRequestHeaders Headers { get; }

}
