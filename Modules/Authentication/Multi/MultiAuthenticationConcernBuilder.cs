using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Modules.Authentication.ApiKey;
using GenHTTP.Modules.Authentication.Basic;
using GenHTTP.Modules.Authentication.Bearer;
using GenHTTP.Modules.Authentication.ClientCertificate;

namespace GenHTTP.Modules.Authentication.Multi;

/// <summary>
/// Builder for creating a multi-concern authentication handler.
/// </summary>
public sealed class MultiAuthenticationConcernBuilder : IConcernBuilder
{
    #region Fields

    private readonly List<IConcernBuilder> _delegatingConcernsBuilders = [];

    #endregion

    #region Private add functionality

    private MultiAuthenticationConcernBuilder AddIfNotNull(IConcernBuilder concernBuilder)
    {
        if (concernBuilder == null) return this;

        _delegatingConcernsBuilders.Add(concernBuilder);
        return this;
    }

    #endregion

    #region Functionality

    /// <summary>
    /// Add nested API concern builder to this multi concern.
    /// </summary>
    /// <param name="apiKeyBuilder">Nested API key concern builder</param>
    public MultiAuthenticationConcernBuilder Add(ApiKeyConcernBuilder apiKeyBuilder)
        => AddIfNotNull(apiKeyBuilder);

    /// <summary>
    /// Add nested API concern builder to this multi concern.
    /// </summary>
    /// <param name="basicBuilder">Nested Basic concern builder</param>
    public MultiAuthenticationConcernBuilder Add(BasicAuthenticationConcernBuilder basicBuilder)
        => AddIfNotNull(basicBuilder);

    /// <summary>
    /// Add nested API concern builder to this multi concern.
    /// </summary>
    /// <param name="basicKnownUsersBuilder">Nested Basic Known Users concern builder</param>
    public MultiAuthenticationConcernBuilder Add(BasicAuthenticationKnownUsersBuilder basicKnownUsersBuilder)
        => AddIfNotNull(basicKnownUsersBuilder);

    /// <summary>
    /// Add nested API concern builder to this multi concern.
    /// </summary>
    /// <param name="bearerBuilder">Nested Bearer concern builder</param>
    public MultiAuthenticationConcernBuilder Add(BearerAuthenticationConcernBuilder bearerBuilder)
        => AddIfNotNull(bearerBuilder);

    /// <summary>
    /// Add nested API concern builder to this multi concern.
    /// </summary>
    /// <param name="clientCertificateBuilder">Nested Client Certificate concern builder</param>
    public MultiAuthenticationConcernBuilder Add(ClientCertificateAuthenticationBuilder clientCertificateBuilder)
        => AddIfNotNull(clientCertificateBuilder);

    /// <summary>
    /// Construct the multi concern.
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public IConcern Build(IHandler content)
    {
        var delegatingConcerns = _delegatingConcernsBuilders.Select(x => x.Build(content)).ToArray();
        if (delegatingConcerns.Length == 0) throw new BuilderMissingPropertyException("Concerns");

        return new MultiAuthenticationConcern(
            content,
            delegatingConcerns
            );
    }

    #endregion

}
