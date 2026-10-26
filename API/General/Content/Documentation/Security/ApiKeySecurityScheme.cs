namespace GenHTTP.Api.Content.Documentation.Security;

/// <summary>
/// The client authenticates by sending a key with the request.
/// </summary>
public sealed record ApiKeySecurityScheme : SecurityScheme
{

    #region Get-/Setters

    /// <summary>
    /// The name of the header, query parameter or cookie the key is sent with.
    /// </summary>
    public string Parameter { get; init; }

    /// <summary>
    /// The part of the request the key is sent with.
    /// </summary>
    public ApiKeyLocation Location { get; init; }

    #endregion

    #region Initialization

    internal ApiKeySecurityScheme(string parameter, ApiKeyLocation location, string? description) : base("apiKey", description)
    {
        Parameter = parameter;
        Location = location;
    }

    #endregion

}
