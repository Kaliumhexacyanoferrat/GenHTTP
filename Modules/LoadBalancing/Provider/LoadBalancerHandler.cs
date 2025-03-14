﻿using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.LoadBalancing.Provider;

public sealed class LoadBalancerHandler : IHandler
{

    #region Get-/Setters

    private readonly List<(IHandler, PriorityEvaluation)> _Nodes;

    private static readonly Random Random = new();

    #endregion

    #region Initialization

    public LoadBalancerHandler(List<(IHandlerBuilder, PriorityEvaluation)> nodes)
    {
        _Nodes = nodes.Select(n => (n.Item1.Build(), n.Item2)).ToList();
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
                                  .MaxBy(n => n.Key)?
                                  .Select(n => n.Item1)
                                  .ToList();

        if (priorityGroup is not null)
        {
            // let a random one handle the request
            if (priorityGroup.Count > 1)
            {
                var index = Random.Next(0, priorityGroup.Count);

                return priorityGroup[index].HandleAsync(request);
            }

            return priorityGroup[0].HandleAsync(request);
        }

        return new ValueTask<IResponse?>();
    }

    #endregion

}
