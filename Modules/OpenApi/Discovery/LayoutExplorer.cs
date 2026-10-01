using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Layouting.Provider;

using NSwag;

namespace GenHTTP.Modules.OpenApi.Discovery;

public sealed class LayoutExplorer : IApiExplorer
{

    public bool CanExplore(IHandler handler) => handler is LayoutHandler;

    public async ValueTask ExploreAsync(IRequest request, IHandler handler, List<string> path, OpenApiDocument document, SchemaManager schemata, InheritedDocumentation documentation, ApiDiscoveryRegistry registry)
    {
        if (handler is LayoutHandler layout)
        {
            foreach (var root in layout.RootHandlers)
            {
                await registry.ExploreAsync(request, root, path, document, schemata, documentation);
            }

            foreach (var (route, routeHandler) in layout.RoutedHandlers)
            {
                await registry.ExploreAsync(request, routeHandler, [..path, route.Decode()], document, schemata, documentation);
            }
        }
    }

}
