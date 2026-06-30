namespace GenHTTP.Api.Protocol;

/// <summary>
/// A HTTP response to be sent to a connected client.
/// </summary>
public interface IResponse
{

    /// <summary>
    /// The status of the HTTP response.
    /// </summary>
    ResponseStatus Status { get; }

    /// <summary>
    /// How the server should proceed with the client connection.
    /// </summary>
    Connection Mode { get; }

    /// <summary>
    /// The HTTP headers to be sent to the client.
    /// </summary>
    IKeyValueList Headers { get; }

    /// <summary>
    /// The content of the response, if any.
    /// </summary>
    IResponseContent? Content { get; }

    /// <summary>
    /// Call to get access to the builder which allows to
    /// modify the response as needed.
    /// </summary>
    /// <returns>The response builder which can be used to modify the response</returns>
    IResponseBuilder Rebuild();

}
