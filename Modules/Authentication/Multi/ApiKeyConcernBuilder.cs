using GenHTTP.Api.Content;
using GenHTTP.Modules.Authentication.ApiKey;
using GenHTTP.Modules.Authentication.Basic;
using GenHTTP.Modules.Authentication.Bearer;
using GenHTTP.Modules.Authentication.ClientCertificate;

namespace GenHTTP.Modules.Authentication.Multi;

public sealed class MultiConcernBuilder : IConcernBuilder
{
    private readonly List<IConcernBuilder> _delegatingConcernsBuilders = [];

    #region Functionality

    /// <summary>
    /// Add nested API concern builder to this multi concern.
    /// </summary>
    /// <param name="apiKeyBuilder">Nested API key concern builder</param>
    public MultiConcernBuilder Add(ApiKeyConcernBuilder apiKeyBuilder)
    {
        _delegatingConcernsBuilders.Add(apiKeyBuilder);
        return this;
    }

    public MultiConcernBuilder Add(BasicAuthenticationConcernBuilder apiKeyBuilder)
    {
        _delegatingConcernsBuilders.Add(apiKeyBuilder);
        return this;
    }

    public MultiConcernBuilder Add(BasicAuthenticationKnownUsersBuilder apiKeyBuilder)
    {
        _delegatingConcernsBuilders.Add(apiKeyBuilder);
        return this;
    }

    public MultiConcernBuilder Add(BearerAuthenticationConcernBuilder apiKeyBuilder)
    {
        _delegatingConcernsBuilders.Add(apiKeyBuilder);
        return this;
    }

    public MultiConcernBuilder Add(ClientCertificateAuthenticationBuilder apiKeyBuilder)
    {
        _delegatingConcernsBuilders.Add(apiKeyBuilder);
        return this;
    }

    public IConcern Build(IHandler content)
    {
        return new MultiConcern(
            content,
            _delegatingConcernsBuilders.Select(x => x.Build(content)).ToArray()
            );
    }

    #endregion

}
