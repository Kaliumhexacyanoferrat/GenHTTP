using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;

namespace GenHTTP.Modules.Authentication;

/// <summary>
/// A protocol for builders that create authentication concerns, allowing to
/// configure how the authentication mechanism is presented to the consumers of
/// the API (e.g. within a generated OpenAPI specification).
/// </summary>
/// <typeparam name="T">The builder type to be returned</typeparam>
public interface IAuthenticationBuilder<out T> : IConcernBuilder where T : IAuthenticationBuilder<T>
{

    /// <summary>
    /// Describes how clients obtain their credentials (e.g. where to request an API key).
    /// </summary>
    /// <param name="description">The description to be shown to the consumers of the API (may contain Markdown)</param>
    /// <remarks>
    /// The description is added to the scheme derived from the configuration of the concern
    /// or to the scheme passed to <see cref="Documentation(SecurityScheme)" />, regardless of
    /// the order of the calls.
    /// </remarks>
    T Documentation(string description);

    /// <summary>
    /// Documents the authentication mechanism as the given scheme, instead of the one
    /// derived from the configuration of the concern.
    /// </summary>
    /// <param name="scheme">The scheme to be shown to the consumers of the API</param>
    /// <remarks>
    /// Use this if the concern cannot derive the mechanism on its own (e.g. if credentials
    /// are read by custom logic) or to document it differently (e.g. to point to the OpenID
    /// Connect provider issuing bearer tokens, see <see cref="SecurityScheme.OpenIdConnect" />).
    /// </remarks>
    T Documentation(SecurityScheme scheme);

}
