using GenHTTP.Api.Content;

using GenHTTP.Modules.Layouting.Provider;
using GenHTTP.Modules.OpenApi.Handler;
using NSwag;

namespace GenHTTP.Modules.OpenApi.Discovery;

public sealed class LayoutExplorer : IApiExplorer
{

    public bool CanExplore(IHandler handler) => handler is LayoutRouter;

    public void Explore(IHandler handler, List<string> path, OpenApiDocument document, SchemaManager schemata, ApiDiscoveryRegistry registry)
    {
        if (handler is LayoutRouter layout)
        {
            foreach (var root in layout.RootHandlers)
            {
                registry.Explore(root, path, document, schemata);
            }

            foreach (var (route, routeHandler) in layout.RoutedHandlers)
            {
                registry.Explore(routeHandler, [..path, route], document, schemata);
            }
        }
    }

}
