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
