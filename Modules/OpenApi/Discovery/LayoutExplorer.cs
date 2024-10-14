using GenHTTP.Api.Content;

using GenHTTP.Modules.Layouting.Provider;
using Microsoft.OpenApi.Models;

namespace GenHTTP.Modules.OpenApi.Discovery;

public sealed class LayoutExplorer : IApiExplorer
{

    public bool CanExplore(IHandler handler) => handler is LayoutRouter;

    public void Explore(IHandler handler, List<string> path, OpenApiDocument document, ApiDiscoveryRegistry registry)
    {
        if (handler is LayoutRouter layout)
        {
            foreach (var root in layout.RootHandlers)
            {
                registry.Explore(root, path, document);
            }

            foreach (var (route, routeHandler) in layout.RoutedHandlers)
            {
                registry.Explore(routeHandler, [..path, route], document);
            }
        }
    }

}
