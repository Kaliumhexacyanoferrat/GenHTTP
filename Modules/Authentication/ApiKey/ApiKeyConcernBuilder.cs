using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Authentication.ApiKey;

public sealed class ApiKeyConcernBuilder : IConcernBuilder
{
    private Func<IRequest, string, ValueTask<IUser?>>? _Authenticator;

    private Func<IRequest, string?>? _KeyExtractor = request => request.Headers.GetValueOrDefault("X-API-Key");

    #region Functionality

    /// <summary>
    /// Configures the logic that is used to extract an API key from
    /// an incoming request (e.g. by reading a header value).
    /// </summary>
    /// <param name="keyExtractor">The logic to be executed to fetch an API key from a request</param>
    public ApiKeyConcernBuilder Extractor(Func<IRequest, string?> keyExtractor)
    {
        _KeyExtractor = keyExtractor;
        return this;
    }

    /// <summary>
    /// Configures the handler to read the API key from the
    /// specified HTTP header.
    /// </summary>
    /// <param name="headerName">The name of the header to be read from the request</param>
    public ApiKeyConcernBuilder WithHeader(string headerName)
    {
        _KeyExtractor = request => request.Headers.GetValueOrDefault(headerName);
        return this;
    }

    /// <summary>
    /// Configures the handler to read the API key from the
    /// specified query parameter.
    /// </summary>
    /// <param name="headerName">The name of the query parameter to be read from the request</param>
    public ApiKeyConcernBuilder WithQueryParameter(string parameter)
    {
        _KeyExtractor = request => request.Query.GetValueOrDefault(parameter);
        return this;
    }

    /// <summary>
    /// Configures the logic that checks whether a given API key
    /// is valid.
    /// </summary>
    /// <param name="authenticator">The logic to be executed to authenticate a request</param>
    public ApiKeyConcernBuilder Authenticator(Func<IRequest, string, ValueTask<IUser?>> authenticator)
    {
        _Authenticator = authenticator;
        return this;
    }

    /// <summary>
    /// Configures the handler to accept any of the given API keys.
    /// </summary>
    /// <param name="allowedKeys">The keys which are allowed to access the content secured by the concern</param>
    public ApiKeyConcernBuilder Keys(params string[] allowedKeys)
    {
        var keySet = new HashSet<string>(allowedKeys);

        _Authenticator = (_, key) => keySet.Contains(key) ? new ValueTask<IUser?>(new ApiKeyUser(key)) : new ValueTask<IUser?>();

        return this;
    }

    public IConcern Build(IHandler parent, Func<IHandler, IHandler> contentFactory)
    {
        var keyExtractor = _KeyExtractor ?? throw new BuilderMissingPropertyException("KeyExtractor");

        var authenticator = _Authenticator ?? throw new BuilderMissingPropertyException("Authenticator");

        return new ApiKeyConcern(parent, contentFactory, keyExtractor, authenticator);
    }

    #endregion

}
