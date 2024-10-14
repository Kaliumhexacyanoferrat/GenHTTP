using GenHTTP.Api.Content;

using GenHTTP.Modules.Layouting.Provider;

namespace GenHTTP.Modules.OpenApi.Discovery;

public sealed class LayoutExplorer : IApiExplorer
{

    public bool CanExplore(IHandler handler) => handler is LayoutRouter;

    public void Explore(IHandler handler, List<string> path, ApiDiscoveryRegistry registry)
    {
        if (handler is LayoutRouter layout)
        {
            foreach (var root in layout.RootHandlers)
            {
                registry.Explore(root, path);
            }

            foreach (var (route, routeHandler) in layout.RoutedHandlers)
            {
                registry.Explore(routeHandler, [..path, route]);
            }
        }
    }

}
