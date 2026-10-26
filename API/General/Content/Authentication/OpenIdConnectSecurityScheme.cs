namespace GenHTTP.Api.Content.Authentication;

/// <summary>
/// The client authenticates with a token obtained from an OpenID Connect provider.
/// </summary>
public sealed record OpenIdConnectSecurityScheme : SecurityScheme
{

    #region Get-/Setters

    /// <summary>
    /// The URL of the discovery document of the provider.
    /// </summary>
    public string DiscoveryUrl { get; init; }

    #endregion

    #region Initialization

    internal OpenIdConnectSecurityScheme(string discoveryUrl, string? description) : base("openIdConnect", description)
    {
        DiscoveryUrl = discoveryUrl;
    }

    #endregion

}
