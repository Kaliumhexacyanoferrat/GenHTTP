using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Content.Documentation.Security;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Authentication.Documentation;

namespace GenHTTP.Modules.Authentication.ApiKey;

public sealed class ApiKeyConcernBuilder : IAuthenticationBuilder<ApiKeyConcernBuilder>
{
    private const string DefaultHeader = "X-API-Key";

    private readonly SchemeDocumentation _documentation = new();

    private Func<IRequest, string, ValueTask<IUser?>>? _authenticator;

    private Func<IRequest, string?>? _keyExtractor = request => request.Header.Headers.GetEntry(DefaultHeader);

    private SecurityScheme? _inferredScheme = SecurityScheme.ApiKey(DefaultHeader);

    #region Functionality

    /// <summary>
    /// Configures the logic that is used to extract an API key from
    /// an incoming request (e.g. by reading a header value).
    /// </summary>
    /// <param name="keyExtractor">The logic to be executed to fetch an API key from a request</param>
    /// <remarks>
    /// As the concern cannot know where the logic reads the key from, it will not be
    /// documented in a generated API specification unless you describe it with
    /// <see cref="Documentation(SecurityScheme)" />.
    /// </remarks>
    public ApiKeyConcernBuilder Extractor(Func<IRequest, string?> keyExtractor)
    {
        _keyExtractor = keyExtractor;
        _inferredScheme = null;
        return this;
    }

    /// <summary>
    /// Configures the handler to read the API key from the
    /// specified HTTP header.
    /// </summary>
    /// <param name="headerName">The name of the header to be read from the request</param>
    public ApiKeyConcernBuilder WithHeader(string headerName)
    {
        _keyExtractor = request => request.Header.Headers.GetEntry(headerName);
        _inferredScheme = SecurityScheme.ApiKey(headerName);
        return this;
    }

    /// <summary>
    /// Configures the handler to read the API key from the
    /// specified query parameter.
    /// </summary>
    /// <param name="parameter">The name of the query parameter to be read from the request</param>
    public ApiKeyConcernBuilder WithQueryParameter(string parameter)
    {
        _keyExtractor = request => request.Header.Query.GetEntry(parameter);
        _inferredScheme = SecurityScheme.ApiKey(parameter, ApiKeyLocation.Query);
        return this;
    }

    /// <summary>
    /// Configures the handler to read the API key from the
    /// specified cookie.
    /// </summary>
    /// <param name="cookie">The name of the cookie to be read from the request</param>
    public ApiKeyConcernBuilder WithCookie(string cookie)
    {
        _keyExtractor = request => request.Header.Headers.GetCookie(cookie);
        _inferredScheme = SecurityScheme.ApiKey(cookie, ApiKeyLocation.Cookie);
        return this;
    }

    /// <summary>
    /// Configures the logic that checks whether a given API key
    /// is valid.
    /// </summary>
    /// <param name="authenticator">The logic to be executed to authenticate a request</param>
    public ApiKeyConcernBuilder Authenticator(Func<IRequest, string, ValueTask<IUser?>> authenticator)
    {
        _authenticator = authenticator;
        return this;
    }

    /// <summary>
    /// Configures the handler to accept any of the given API keys.
    /// </summary>
    /// <param name="allowedKeys">The keys which are allowed to access the content secured by the concern</param>
    public ApiKeyConcernBuilder Keys(params string[] allowedKeys)
    {
        var keySet = new HashSet<string>(allowedKeys);

        _authenticator = (_, key) => keySet.Contains(key) ? new ValueTask<IUser?>(new ApiKeyUser(key)) : new ValueTask<IUser?>();

        return this;
    }

    public ApiKeyConcernBuilder Documentation(string description)
    {
        _documentation.Description = description;
        return this;
    }

    public ApiKeyConcernBuilder Documentation(SecurityScheme scheme)
    {
        _documentation.Scheme = scheme;
        return this;
    }

    public IConcern Build(IHandler content)
    {
        var keyExtractor = _keyExtractor ?? throw new BuilderMissingPropertyException("KeyExtractor");

        var authenticator = _authenticator ?? throw new BuilderMissingPropertyException("Authenticator");

        return new ApiKeyConcern(content, keyExtractor, authenticator, _documentation.ResolveOptional(_inferredScheme));
    }

    #endregion

}
