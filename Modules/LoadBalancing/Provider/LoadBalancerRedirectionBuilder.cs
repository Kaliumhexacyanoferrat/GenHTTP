using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.LoadBalancing.Provider;

public sealed class LoadBalancerRedirectionBuilder : IHandlerBuilder<LoadBalancerRedirectionBuilder>
{
    private readonly List<IConcernBuilder> _Concerns = [];

    private string? _Root;

    #region Functionality

    public LoadBalancerRedirectionBuilder Add(IConcernBuilder concern)
    {
        _Concerns.Add(concern);
        return this;
    }

    public LoadBalancerRedirectionBuilder Root(string node)
    {
        _Root = node;
        return this;
    }

    public IHandler Build()
    {
        var root = _Root ?? throw new BuilderMissingPropertyException("root");

        return Concerns.Chain(_Concerns,  new LoadBalancerRedirectionHandler( root));
    }

    #endregion

}
