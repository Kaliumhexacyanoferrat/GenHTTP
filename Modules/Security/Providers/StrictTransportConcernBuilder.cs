using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.Security.Providers;

public sealed class StrictTransportConcernBuilder : IConcernBuilder
{
    private StrictTransportPolicy? _Policy;

    #region Functionality

    public StrictTransportConcernBuilder Policy(StrictTransportPolicy policy)
    {
        _Policy = policy;
        return this;
    }

    public IConcern Build(IHandler content)
    {
        var policy = _Policy ?? throw new BuilderMissingPropertyException("policy");

        return new StrictTransportConcern(content, policy);
    }

    #endregion

}
