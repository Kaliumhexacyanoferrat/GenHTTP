namespace GenHTTP.Api.Protocol;

/// <summary>
/// The attributes that can be set on a cookie in addition to its
/// name and value (see RFC 6265).
/// </summary>
public readonly struct CookieOptions
{

    /// <summary>
    /// The duration after which the client should discard the cookie.
    /// </summary>
    /// <remarks>
    /// This is the modern, recommended way to expire a cookie - see the
    /// remarks on <see cref="Expires"/> for how the two attributes interact.
    /// </remarks>
    public TimeSpan? MaxAge { get; init; }


    /// <summary>
    /// The point in time after which the client should discard the cookie.
    /// </summary>
    /// <remarks>
    /// This is the legacy attribute for cookie expiration, kept for clients
    /// that predate <see cref="MaxAge"/>. Prefer <see cref="MaxAge"/> instead,
    /// as it is relative to the time the cookie is received and therefore not
    /// affected by clock drift between client and server. If both attributes
    /// are set, RFC 6265 requires clients that understand <see cref="MaxAge"/>
    /// to prefer it and ignore this value - this type does not enforce that
    /// exclusivity, so setting both will result in both being sent.
    /// </remarks>
    public DateTimeOffset? Expires { get; init; }
    
    /// <summary>
    /// The hosts the cookie should be sent to.
    /// </summary>
    /// <remarks>
    /// Setting this attribute also makes the cookie available to subdomains
    /// of the given host, which widens the audience that can read the cookie.
    /// If left unset, the cookie is only sent back to the exact host that
    /// originally set it (and not to subdomains), which is the safer default
    /// for most use cases.
    /// </remarks>
    public ByteString? Domain { get; init; }

    /// <summary>
    /// The path that must be present in the request URL for the cookie to be sent.
    /// </summary>
    /// <remarks>
    /// Unlike a browser receiving this header, this type does not default the
    /// path to the path of the request the cookie was set on - if left unset,
    /// no Path attribute is sent and the cookie is scoped by the client according
    /// to its own default behavior (typically the path of the request).
    /// </remarks>
    public ByteString? Path { get; init; }

    /// <summary>
    /// Whether the cookie should only be sent to the server via encrypted connections.
    /// </summary>
    /// <remarks>
    /// Should be set whenever possible to prevent the cookie from being exposed
    /// over plain HTTP. This is also a requirement for <see cref="GenHTTP.Api.Protocol.SameSite.None"/>,
    /// which most clients will otherwise reject.
    /// </remarks>
    public bool Secure { get; init; }

    /// <summary>
    /// Whether the cookie should be hidden from client side scripts.
    /// </summary>
    /// <remarks>
    /// Should be set whenever the cookie does not need to be read by JavaScript,
    /// as it mitigates the impact of cross-site scripting (XSS) attacks by
    /// keeping session identifiers and similar values out of reach of scripts.
    /// </remarks>
    public bool HttpOnly { get; init; }

    /// <summary>
    /// Restricts whether the cookie is sent along with cross-site requests.
    /// </summary>
    /// <remarks>
    /// Leaving this unset relies on the client's default behavior, which varies
    /// across clients and may change over time (most current browsers default to
    /// <see cref="GenHTTP.Api.Protocol.SameSite.Lax"/>). Set this explicitly to
    /// get a predictable, future-proof outcome. Note that <see cref="GenHTTP.Api.Protocol.SameSite.None"/>
    /// additionally requires <see cref="Secure"/> to be set, or the cookie will
    /// be rejected by most clients.
    /// </remarks>
    public SameSite? SameSite { get; init; }

}
