using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.LoadBalancing.Provider
{

    public sealed class LoadBalancerHandler : IHandler
    {

        #region Get-/Setters

        public IHandler Parent { get; }

        private readonly List<(IHandler, PriorityEvaluation)> _Nodes;

        private static readonly Random _Random = new();

        #endregion

        #region Initialization

        public LoadBalancerHandler(IHandler parent, List<(IHandlerBuilder, PriorityEvaluation)> nodes)
        {
            Parent = parent;

            _Nodes = nodes.Select(n => (n.Item1.Build(this), n.Item2)).ToList();
        }

        #endregion

        #region Functionality

        public async ValueTask PrepareAsync()
        {
            foreach (var entry in _Nodes)
            {
                await entry.Item1.PrepareAsync();
            }
        }

        public ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            // get the handlers that share the highest priority
            var priorityGroup = _Nodes.GroupBy(n => n.Item2(request))
                                      .OrderByDescending(n => n.Key)
                                      .FirstOrDefault()?
                                      .Select(n => n.Item1)
                                      .ToList();

            if (priorityGroup is not null)
            {
                // let a random one handle the request
                if (priorityGroup.Count > 1)
                {
                    var index = _Random.Next(0, priorityGroup.Count);

                    return priorityGroup[index].HandleAsync(request);
                }
                else
                {
                    return priorityGroup.First()
                                        .HandleAsync(request);
                }
            }

            return new ValueTask<IResponse?>();
        }

        public IAsyncEnumerable<ContentElement> GetContentAsync(IRequest request) => AsyncEnumerable.Empty<ContentElement>();

        #endregion

    }

}
