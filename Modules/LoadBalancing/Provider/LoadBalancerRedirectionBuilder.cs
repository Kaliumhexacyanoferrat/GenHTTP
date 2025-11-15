using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.LoadBalancing.Provider;

public sealed class LoadBalancerRedirectionBuilder : IHandlerBuilder<LoadBalancerRedirectionBuilder>
{
    private readonly List<IConcernBuilder> _concerns = [];

    private string? _root;

    #region Functionality

    public LoadBalancerRedirectionBuilder Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);
        return this;
    }

    public LoadBalancerRedirectionBuilder Root(string node)
    {
        _root = node;
        return this;
    }

    public IHandler Build()
    {
        var root = _root ?? throw new BuilderMissingPropertyException("root");

        return Concerns.Chain(_concerns,  new LoadBalancerRedirectionHandler( root));
    }

    #endregion

}
