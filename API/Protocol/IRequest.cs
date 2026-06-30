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
    /// The remote client that issued this request.
    /// </summary>
    IClientConnection Client { get; }

    /// <summary>
    /// Property bag to store values during the lifetime of the request.
    /// </summary>
    IPropertyBag Properties { get; }

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

    /// <summary>
    /// Registers a wrapper function that will be used to construct
    /// the actual request body instance when requested.
    /// </summary>
    /// <remarks>
    /// Allows concerns to transparently replace the request body
    /// to implement features such as automatic request body
    /// decompression. Note, that only one wrapper can be active.
    /// </remarks>
    /// <param name="wrapper">The wrapper to apply when the body is retrieved</param>
    void WrapBody(Func<IRequestBody, IRequestBody> wrapper);

    /// <summary>
    /// Creates a response for this request.
    /// </summary>
    /// <returns>The response which can be returned by handlers to answer this request</returns>
    IResponseBuilder Respond();

}
