namespace GenHTTP.Api.Content.Documentation.Security;

/// <summary>
/// Requires the client to authenticate with any one of the given schemes.
/// </summary>
/// <param name="AnyOf">The schemes the client may choose from</param>
public sealed record AuthenticationRequirement(IReadOnlyList<SecurityScheme> AnyOf);
