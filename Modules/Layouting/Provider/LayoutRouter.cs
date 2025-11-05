using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Redirects;

namespace GenHTTP.Modules.Layouting.Provider;

public sealed class LayoutRouter : IHandler
{

    #region Get-/Setters

    public IReadOnlyDictionary<string, IHandler> RoutedHandlers { get; }

    public IReadOnlyList<IHandler> RootHandlers { get; }

    public IHandler? Index { get; }

    #endregion

    #region Initialization

    public LayoutRouter(Dictionary<string, IHandler> routedHandlers, List<IHandler> rootHandlers, IHandler? index)
    {
        RoutedHandlers = routedHandlers;
        RootHandlers = rootHandlers;
        Index = index;
    }

    #endregion

    #region Functionality

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var current = request.Target.Current;

        if (current is not null)
        {
            if (RoutedHandlers.TryGetValue(current.Value, out var handler))
            {
                request.Target.Advance();

                return await handler.HandleAsync(request);
            }
        }
        else
        {
            // force a trailing slash to prevent duplicate content
            if (!request.Target.Path.TrailingSlash)
            {
                return await Redirect.To($"{request.Target.Path}/")
                                     .Build()
                                     .HandleAsync(request);
            }

            if (Index is not null)
            {
                return await Index.HandleAsync(request);
            }
        }

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

    public async ValueTask PrepareAsync()
    {
        if (Index != null)
        {
            await Index.PrepareAsync();
        }

        foreach (var handler in RoutedHandlers.Values)
        {
            await handler.PrepareAsync();
        }

        foreach (var handler in RootHandlers)
        {
            await handler.PrepareAsync();
        }
    }

    #endregion

}
