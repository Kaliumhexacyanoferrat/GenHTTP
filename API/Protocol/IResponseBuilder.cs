using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Api.Protocol;

/// <summary>
/// Allows to create and modify responses.
/// </summary>
public interface IResponseBuilder : IBuilder<IResponse>
{

    /// <summary>
    /// Sets the status of the HTTP response.
    /// </summary>
    /// <param name="status">The HTTP status code to respond with</param>
    /// <returns>The builder instance</returns>
    IResponseBuilder Status(ResponseStatus status);

    /// <summary>
    /// Configures how the server should treat the client connection.
    /// </summary>
    /// <param name="mode">The mode the server should apply</param>
    /// <returns>The builder instance</returns>
    IResponseBuilder Connection(Connection mode);

    /// <summary>
    /// Sets a HTTP header on the response.
    /// </summary>
    /// <param name="name">The name of the header to be set</param>
    /// <param name="value">The value of the header to be set</param>
    /// <returns>The builder instance</returns>
    IResponseBuilder Header(ByteString name, ByteString value);

    /// <summary>
    /// Sets a HTTP header on the response.
    /// </summary>
    /// <param name="name">The name of the header to be set</param>
    /// <param name="value">The value of the header to be set</param>
    /// <returns>The builder instance</returns>
    IResponseBuilder Header(string name, string value);

    /// <summary>
    /// Sets the content of the HTTP response.
    /// </summary>
    /// <param name="content">The content to be set</param>
    /// <returns>The builder instance</returns>
    IResponseBuilder Content(IResponseContent? content);

}
