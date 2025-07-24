using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Reflection;

using NSwag;

namespace GenHTTP.Modules.OpenApi.Discovery;

public sealed class MethodCollectionExplorer : IApiExplorer
{

    public bool CanExplore(IHandler handler) => handler is MethodCollection;

    public async ValueTask ExploreAsync(IRequest request, IHandler handler, List<string> path, OpenApiDocument document, SchemaManager schemata, ApiDiscoveryRegistry registry)
    {
        if (handler is MethodCollection collection)
        {
            foreach (var method in collection.Methods)
            {
                await registry.ExploreAsync(request, method, path, document, schemata);
            }
        }
    }
}
