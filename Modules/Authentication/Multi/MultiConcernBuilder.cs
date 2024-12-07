using GenHTTP.Api.Content;
using GenHTTP.Modules.Authentication.ApiKey;
using GenHTTP.Modules.Authentication.Basic;
using GenHTTP.Modules.Authentication.Bearer;
using GenHTTP.Modules.Authentication.ClientCertificate;

namespace GenHTTP.Modules.Authentication.Multi;

/// <summary>
/// Builder for creating a multi-concern authentication handler.
/// </summary>
public sealed class MultiConcernBuilder : IConcernBuilder
{
    #region Fields

    private readonly List<IConcernBuilder> _delegatingConcernsBuilders = [];

    #endregion

    #region Private add functionality

    private MultiConcernBuilder AddIfNotNull(IConcernBuilder concernBuilder)
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
    public MultiConcernBuilder Add(ApiKeyConcernBuilder apiKeyBuilder)
        => AddIfNotNull(apiKeyBuilder);

    /// <summary>
    /// Add nested API concern builder to this multi concern.
    /// </summary>
    /// <param name="basicBuilder">Nested Basic concern builder</param>
    public MultiConcernBuilder Add(BasicAuthenticationConcernBuilder basicBuilder)
        => AddIfNotNull(basicBuilder);

    /// <summary>
    /// Add nested API concern builder to this multi concern.
    /// </summary>
    /// <param name="basicKnownUsersBuilder">Nested Basic Known Users concern builder</param>
    public MultiConcernBuilder Add(BasicAuthenticationKnownUsersBuilder basicKnownUsersBuilder)
        => AddIfNotNull(basicKnownUsersBuilder);

    /// <summary>
    /// Add nested API concern builder to this multi concern.
    /// </summary>
    /// <param name="bearerBuilder">Nested Bearer concern builder</param>
    public MultiConcernBuilder Add(BearerAuthenticationConcernBuilder bearerBuilder)
        => AddIfNotNull(bearerBuilder);

    /// <summary>
    /// Add nested API concern builder to this multi concern.
    /// </summary>
    /// <param name="clientCertificateBuilder">Nested Client Certificate concern builder</param>
    public MultiConcernBuilder Add(ClientCertificateAuthenticationBuilder clientCertificateBuilder)
        => AddIfNotNull(clientCertificateBuilder);

    /// <summary>
    /// Construct the multi concern.
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public IConcern Build(IHandler content)
    {
        return new MultiConcern(
            content,
            _delegatingConcernsBuilders.Select(x => x.Build(content)).ToArray()
            );
    }

    #endregion

}
