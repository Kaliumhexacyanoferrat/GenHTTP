using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Api.Protocol;

/// <summary>
/// Represents an incoming HTTP request to be handled by the server.
/// </summary>
public interface IRequest
{

    /// <summary>
    /// The server instance handling the request.
    /// </summary>
    IServer Server { get; }

    /// <summary>
    /// The endpoint the request was sent to.
    /// </summary>
    IEndPoint EndPoint { get; }

    /// <summary>
    /// Property bag to store values during the lifetime of the request.
    /// </summary>
    IRequestProperties Properties { get; }

    /// <summary>
    /// The header of the HTTP request.
    /// </summary>
    IRequestHeader Header { get;}

    /// <summary>
    /// Attempts to fetch the body of the HTTP request (if any). Can only be called once.
    /// </summary>
    /// <param name="headerAccess">Specifies access to the header should be retained or not</param>
    /// <returns>The body of the HTTP request (if any)</returns>
    IRequestBody? GetBody(HeaderAccess headerAccess = HeaderAccess.Retain);

    IResponseBuilder Respond();
    
    // todo: wrap body (e.g. content decoding)

}

