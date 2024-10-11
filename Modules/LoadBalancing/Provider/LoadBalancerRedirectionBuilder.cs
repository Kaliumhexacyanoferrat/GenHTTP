using System.Collections.Generic;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.LoadBalancing.Provider;

public sealed class LoadBalancerRedirectionBuilder : IHandlerBuilder<LoadBalancerRedirectionBuilder>
{
    private readonly List<IConcernBuilder> _Concerns = new();

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

    public IHandler Build(IHandler parent)
    {
            var root = _Root ?? throw new BuilderMissingPropertyException("root");

            return Concerns.Chain(parent, _Concerns, (p) => new LoadBalancerRedirectionHandler(p, root));
        }

    #endregion

}
