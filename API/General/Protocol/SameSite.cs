namespace GenHTTP.Api.Protocol;

/// <summary>
/// Controls whether a cookie is sent along with cross-site requests.
/// </summary>
public enum SameSite
{

    /// <summary>
    /// The cookie will only be sent in a first-party context.
    /// </summary>
    Strict,

    /// <summary>
    /// The cookie will be sent along with top-level navigations.
    /// </summary>
    Lax,

    /// <summary>
    /// The cookie will be sent with both cross-site and same-site requests.
    /// </summary>
    None

}
