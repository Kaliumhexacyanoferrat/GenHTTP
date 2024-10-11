using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Security.Cors;

public sealed class CorsPolicyBuilder : IConcernBuilder
{
    private readonly Dictionary<string, OriginPolicy?> _AdditionalPolicies = new(4);

    private OriginPolicy? _DefaultPolicy;

    #region Functionality

    /// <summary>
    /// Sets the default policy to be applied, if no origin is given or
    /// </summary>
    /// <param name="policy"></param>
    /// <returns></returns>
    public CorsPolicyBuilder Default(OriginPolicy? policy)
    {
        _DefaultPolicy = policy;
        return this;
    }

    /// <summary>
    /// Adds a custom policy for the specified origin.
    /// </summary>
    /// <param name="origin">The origin the policy applies to (e.g. https://example.com)</param>
    /// <param name="policy">The policy to be applied (if not set, access will be denied)</param>
    public CorsPolicyBuilder Add(string origin, OriginPolicy? policy)
    {
        _AdditionalPolicies[origin] = policy;
        return this;
    }

    /// <summary>
    /// Adds a custom policy for the specified origin.
    /// </summary>
    /// <param name="origin">The origin the policy applies to (e.g. https://example.com)</param>
    /// <param name="allowedMethods">The HTTP methods the client is allowed to access (any, if not given)</param>
    /// <param name="allowedHeaders">The headers a client may send to the server (any, if not given)</param>
    /// <param name="exposedHeaders">The headers that will be accessible by the client (any, if not given)</param>
    /// <param name="allowCredentials">Whether the client is allowed to read credentials from the request</param>
    /// <param name="maxAge">The duration in seconds this policy is valid for</param>
    public CorsPolicyBuilder Add(string origin, List<FlexibleRequestMethod>? allowedMethods, List<string>? allowedHeaders,
        List<string>? exposedHeaders, bool allowCredentials, uint maxAge = 86400) => Add(origin, new OriginPolicy(allowedMethods, allowedHeaders, exposedHeaders, allowCredentials, maxAge));

    public IConcern Build(IHandler parent, Func<IHandler, IHandler> contentFactory) => new CorsPolicyHandler(parent, contentFactory, _DefaultPolicy, _AdditionalPolicies);

    #endregion

}
