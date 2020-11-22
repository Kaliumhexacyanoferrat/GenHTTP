using System.Collections.Generic;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.LoadBalancing.Provider
{

    public sealed class LoadBalancerBuilder : IHandlerBuilder<LoadBalancerBuilder>
    {
        private readonly List<IConcernBuilder> _Concerns = new();

        private readonly List<(IHandlerBuilder, PriorityEvaluation)> _Nodes = new();

        private static readonly PriorityEvaluation DEFAULT_PRIORITY = (_) => Priority.Medium;

        #region Functionality

        public LoadBalancerBuilder Add(IConcernBuilder concern)
        {
            _Concerns.Add(concern);
            return this;
        }

        public LoadBalancerBuilder Add(IHandlerBuilder handler, PriorityEvaluation? priority = null)
        {
            _Nodes.Add((handler, priority ?? DEFAULT_PRIORITY));
            return this;
        }

        public LoadBalancerBuilder Redirect(string node, PriorityEvaluation? priority = null) => Add(new LoadBalancerRedirectionBuilder().Root(node), priority);

        public LoadBalancerBuilder Proxy(string node, PriorityEvaluation? priority = null) => Add(ReverseProxy.Proxy.Create().Upstream(node), priority);

        public IHandler Build(IHandler parent)
        {
            return Concerns.Chain(parent, _Concerns, (p) => new LoadBalancerHandler(p, _Nodes));
        }

        #endregion

    }

}
