using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.Security.Providers;

public sealed class StrictTransportConcernBuilder : IConcernBuilder
{
    private StrictTransportPolicy? _policy;

    #region Functionality

    public StrictTransportConcernBuilder Policy(StrictTransportPolicy policy)
    {
        _policy = policy;
        return this;
    }

    public IConcern Build(IHandler content)
    {
        var policy = _policy ?? throw new BuilderMissingPropertyException("policy");

        return new StrictTransportConcern(content, policy);
    }

    #endregion

}
