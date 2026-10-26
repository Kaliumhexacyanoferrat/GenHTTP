namespace GenHTTP.Api.Content.Authentication;

/// <summary>
/// The part of the request an API key is sent with.
/// </summary>
public enum ApiKeyLocation
{

    /// <summary>
    /// The key is sent as an HTTP header.
    /// </summary>
    Header,

    /// <summary>
    /// The key is sent as a query parameter.
    /// </summary>
    Query,

    /// <summary>
    /// The key is sent as a cookie.
    /// </summary>
    Cookie

}
