using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.LoadBalancing.Provider;

public sealed class LoadBalancerBuilder : IHandlerBuilder<LoadBalancerBuilder>
{
    private static readonly PriorityEvaluation DefaultPriority = _ => Priority.Medium;

    private readonly List<IConcernBuilder> _concerns = [];

    private readonly List<(IHandlerBuilder, PriorityEvaluation)> _nodes = [];

    #region Functionality

    public LoadBalancerBuilder Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);
        return this;
    }

    public LoadBalancerBuilder Add(IHandlerBuilder handler, PriorityEvaluation? priority = null)
    {
        _nodes.Add((handler, priority ?? DefaultPriority));
        return this;
    }

    public LoadBalancerBuilder Redirect(string node, PriorityEvaluation? priority = null) => Add(new LoadBalancerRedirectionBuilder().Root(node), priority);

    public LoadBalancerBuilder Proxy(string node, PriorityEvaluation? priority = null) => Add(ReverseProxy.Proxy.Create().Upstream(node), priority);

    public IHandler Build()
    {
        return Concerns.Chain(_concerns,  new LoadBalancerHandler( _nodes));
    }

    #endregion

}
