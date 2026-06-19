namespace GenHTTP.Api.Protocol;

/// <summary>
/// The attributes that can be set on a cookie in addition to its
/// name and value (see RFC 6265).
/// </summary>
public readonly struct CookieOptions
{

    /// <summary>
    /// The point in time after which the client should discard the cookie.
    /// </summary>
    public DateTimeOffset? Expires { get; init; }

    /// <summary>
    /// The duration after which the client should discard the cookie.
    /// </summary>
    public TimeSpan? MaxAge { get; init; }

    /// <summary>
    /// The hosts the cookie should be sent to.
    /// </summary>
    public ByteString? Domain { get; init; }

    /// <summary>
    /// The path that must be present in the request URL for the cookie to be sent.
    /// </summary>
    public ByteString? Path { get; init; }

    /// <summary>
    /// Whether the cookie should only be sent to the server via encrypted connections.
    /// </summary>
    public bool Secure { get; init; }

    /// <summary>
    /// Whether the cookie should be hidden from client side scripts.
    /// </summary>
    public bool HttpOnly { get; init; }

    /// <summary>
    /// Restricts whether the cookie is sent along with cross-site requests.
    /// </summary>
    public SameSite? SameSite { get; init; }

}
