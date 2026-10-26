namespace GenHTTP.Api.Content.Authentication;

/// <summary>
/// Describes how a client authenticates against an API, so this
/// mechanism can be presented to the consumers of the API (e.g. as
/// a security scheme within a generated OpenAPI specification).
/// </summary>
/// <remarks>
/// Use the factory methods (such as <see cref="ApiKey" />) to create
/// an instance. The name of the scheme can be changed with a
/// <c>with</c> expression, e.g. <c>scheme with { Name = "partnerKey" }</c>.
/// </remarks>
public abstract record SecurityScheme
{

    #region Get-/Setters

    /// <summary>
    /// The name the scheme is listed with (e.g. "apiKey").
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Tells the consumers of the API how to authenticate (e.g. where
    /// to obtain credentials). May be rendered as Markdown.
    /// </summary>
    public string? Description { get; init; }

    #endregion

    #region Initialization

    private protected SecurityScheme(string name, string? description)
    {
        Name = name;
        Description = description;
    }

    #endregion

    #region Factories

    /// <summary>
    /// The client authenticates by sending a key with the request.
    /// </summary>
    /// <param name="parameter">The name of the header, query parameter or cookie the key is sent with</param>
    /// <param name="location">The part of the request the key is sent with</param>
    /// <param name="description">Tells the consumers how to authenticate</param>
    public static ApiKeySecurityScheme ApiKey(string parameter, ApiKeyLocation location = ApiKeyLocation.Header, string? description = null)
        => new(parameter, location, description);

    /// <summary>
    /// The client authenticates with a user name and a password (HTTP basic authentication).
    /// </summary>
    /// <param name="description">Tells the consumers how to authenticate</param>
    public static HttpSecurityScheme Basic(string? description = null)
        => new("basic", "basic", null, description);

    /// <summary>
    /// The client authenticates with a token sent as a bearer token in the "Authorization" header.
    /// </summary>
    /// <param name="format">A hint on the format of the token (e.g. "JWT")</param>
    /// <param name="description">Tells the consumers how to authenticate</param>
    public static HttpSecurityScheme Bearer(string? format = null, string? description = null)
        => new("bearer", "bearer", format, description);

    /// <summary>
    /// The client authenticates using the given HTTP authentication scheme (e.g. "digest").
    /// </summary>
    /// <param name="scheme">The name of the HTTP authentication scheme</param>
    /// <param name="description">Tells the consumers how to authenticate</param>
    public static HttpSecurityScheme Http(string scheme, string? description = null)
        => new(scheme.ToLowerInvariant(), scheme, null, description);

    /// <summary>
    /// The client authenticates with a token obtained from an OpenID Connect provider.
    /// </summary>
    /// <param name="discoveryUrl">The URL of the discovery document of the provider (ending with ".well-known/openid-configuration")</param>
    /// <param name="description">Tells the consumers how to authenticate</param>
    public static OpenIdConnectSecurityScheme OpenIdConnect(string discoveryUrl, string? description = null)
        => new(discoveryUrl, description);

    #endregion

}

/// <summary>
/// The client authenticates by sending a key with the request.
/// </summary>
public sealed record ApiKeySecurityScheme : SecurityScheme
{

    /// <summary>
    /// The name of the header, query parameter or cookie the key is sent with.
    /// </summary>
    public string Parameter { get; init; }

    /// <summary>
    /// The part of the request the key is sent with.
    /// </summary>
    public ApiKeyLocation Location { get; init; }

    internal ApiKeySecurityScheme(string parameter, ApiKeyLocation location, string? description) : base("apiKey", description)
    {
        Parameter = parameter;
        Location = location;
    }

}

/// <summary>
/// The client authenticates using an HTTP authentication scheme
/// (such as basic or bearer authentication).
/// </summary>
public sealed record HttpSecurityScheme : SecurityScheme
{

    /// <summary>
    /// The name of the HTTP authentication scheme (e.g. "basic").
    /// </summary>
    public string Scheme { get; init; }

    /// <summary>
    /// For bearer authentication, a hint on the format of the token (e.g. "JWT").
    /// </summary>
    public string? BearerFormat { get; init; }

    internal HttpSecurityScheme(string name, string scheme, string? bearerFormat, string? description) : base(name, description)
    {
        Scheme = scheme;
        BearerFormat = bearerFormat;
    }

}

/// <summary>
/// The client authenticates with a token obtained from an OpenID Connect provider.
/// </summary>
public sealed record OpenIdConnectSecurityScheme : SecurityScheme
{

    /// <summary>
    /// The URL of the discovery document of the provider.
    /// </summary>
    public string DiscoveryUrl { get; init; }

    internal OpenIdConnectSecurityScheme(string discoveryUrl, string? description) : base("openIdConnect", description)
    {
        DiscoveryUrl = discoveryUrl;
    }

}
