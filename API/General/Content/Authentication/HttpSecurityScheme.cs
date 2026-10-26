namespace GenHTTP.Api.Content.Authentication;

/// <summary>
/// The client authenticates using an HTTP authentication scheme
/// (such as basic or bearer authentication).
/// </summary>
public sealed record HttpSecurityScheme : SecurityScheme
{

    #region Get-/Setters

    /// <summary>
    /// The name of the HTTP authentication scheme (e.g. "basic").
    /// </summary>
    public string Scheme { get; init; }

    /// <summary>
    /// For bearer authentication, a hint on the format of the token (e.g. "JWT").
    /// </summary>
    public string? BearerFormat { get; init; }

    #endregion

    #region Initialization

    internal HttpSecurityScheme(string name, string scheme, string? bearerFormat, string? description) : base(name, description)
    {
        Scheme = scheme;
        BearerFormat = bearerFormat;
    }

    #endregion

}
