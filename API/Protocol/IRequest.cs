using System.IO.Pipelines;
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
    /// Creates a response for this request.
    /// </summary>
    /// <returns>The response which can be returned by handlers to answer this request</returns>
    IResponseBuilder Respond();

    /// <summary>
    /// Expresses intention to upgrade the request to another protocol,
    /// granting access to the underlying pipe reader used by the engine.
    /// </summary>
    /// <remarks>
    /// Using the pipe reader in non-upgrade scenarios might cause the server
    /// failing to handle requests as intended.
    /// </remarks>
    /// <returns>The underlying pipe reader</returns>
    PipeReader Upgrade();

    // todo: wrap body (e.g. content decoding)

}
