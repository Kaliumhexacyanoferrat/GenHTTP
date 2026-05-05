using System.Collections.Frozen;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Util;

namespace GenHTTP.Modules.Layouting.Provider;

public sealed class LayoutHandler : IHandler
{

    #region Get-/Setters

    public FrozenDictionary<int, IHandler> RoutedHandlers { get; }

    public IHandler[] RootHandlers { get; }

    public IHandler? Index { get; }

    #endregion

    #region Initialization

    public LayoutHandler(Dictionary<int, IHandler> routedHandlers, List<IHandler> rootHandlers, IHandler? index)
    {
        RoutedHandlers = routedHandlers.ToFrozenDictionary();
        RootHandlers = rootHandlers.ToArray();
        Index = index;
    }

    #endregion

    #region Functionality

    public async ValueTask PrepareAsync()
    {
        // todo: parallelize?

        foreach (var routed in RoutedHandlers.Values)
        {
            await routed.PrepareAsync();
        }

        foreach (var root in RootHandlers)
        {
            await root.PrepareAsync();
        }

        if (Index != null)
        {
            await Index.PrepareAsync();
        }
    }

    public ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var target = request.Raw.Header.Target;

        if (target.Current is not null)
        {
            var hash = target.Current.Value.Hash();

            if (RoutedHandlers.TryGetValue(hash, out var handler))
            {
                target.Advance();

                return handler.HandleAsync(request);
            }
        }
        else if (Index is not null)
        {
            return Index.HandleAsync(request);
        }

        return InvokeRootHandlersAsync(request);
    }

    private async ValueTask<IResponse?> InvokeRootHandlersAsync(IRequest request)
    {
        foreach (var handler in RootHandlers)
        {
            var result = await handler.HandleAsync(request);

            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    #endregion

}
