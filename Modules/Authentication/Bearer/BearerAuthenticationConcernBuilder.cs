using System.IdentityModel.Tokens.Jwt;
using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Protocol;
using Microsoft.IdentityModel.Tokens;

namespace GenHTTP.Modules.Authentication.Bearer;

public sealed class BearerAuthenticationConcernBuilder : IConcernBuilder
{
    private readonly TokenValidationOptions _options = new();

    #region Functionality

    /// <summary>
    /// Sets the expected issuer. Tokens that are not issued by this
    /// party will be declined.
    /// </summary>
    /// <param name="issuer">The URL of the exepcted issuer</param>
    /// <remarks>
    /// Setting the issuer will cause the concern to download and cache
    /// the signing keys that are used to ensure that the party actually
    /// issued a token.
    /// </remarks>
    public BearerAuthenticationConcernBuilder Issuer(string issuer)
    {
        _options.Issuer = issuer;
        return this;
    }

    /// <summary>
    /// Sets the expected audience that should be accepted.
    /// </summary>
    /// <param name="audience">The audience to check for</param>
    public BearerAuthenticationConcernBuilder Audience(string audience)
    {
        _options.Audience = audience;
        return this;
    }

    /// <summary>
    /// Adds a custom validator that can analyze the token read from the
    /// request and can perform additional checks.
    /// </summary>
    /// <param name="validator">The custom validator to be used</param>
    /// <remarks>
    /// This validator will be invoked after the regular checks (such as the
    /// issuer) have been performed.
    /// If you would like to deny user access within a custom validator,
    /// you can throw a <see cref="ProviderException" />.
    /// </remarks>
    public BearerAuthenticationConcernBuilder Validation(Func<JwtSecurityToken, Task> validator)
    {
        _options.CustomValidator = validator;
        return this;
    }
     
    /// <summary>
    /// Adds a custom logic to fetch the signing keys. By default, the keys will be
    /// downloaded from the URL returned by the .well-known information returned by
    /// the issuer.
    /// </summary>
    /// <param name="keyResolver">The logic used to resolve the signing keys for a given incoming token</param>
    public BearerAuthenticationConcernBuilder KeyResolver(Func<JwtSecurityToken, ValueTask<ICollection<SecurityKey>>> keyResolver)
    {
        _options.CustomKeyResolver = keyResolver;
        return this;
    }

    /// <summary>
    /// Optionally register a function that will compute the user that
    /// should be set for the request.
    /// </summary>
    /// <param name="userMapping">The user mapping to be used</param>
    /// <remarks>
    /// The usage of this mechanism allows to inject the user into
    /// service methods via the user injector class. Returning null
    /// within the delegate will not deny user access - if you would
    /// like to prevent such user, you can throw a <see cref="ProviderException" />.
    /// </remarks>
    public BearerAuthenticationConcernBuilder UserMapping(Func<IRequest, JwtSecurityToken, ValueTask<IUser?>> userMapping)
    {
        _options.UserMapping = userMapping;
        return this;
    }

    /// <summary>
    /// If enabled, tokens that have expired or are not valid yet are
    /// still accepted. This should be used for testing purposes only.
    /// </summary>
    public BearerAuthenticationConcernBuilder AllowExpired()
    {
        _options.Lifetime = false;
        return this;
    }

    public IConcern Build(IHandler content) => new BearerAuthenticationConcern(content, _options);

    #endregion

}
