using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Reflection;

using NSwag;

namespace GenHTTP.Modules.OpenApi.Discovery;

public sealed class ServiceExplorer : IApiExplorer
{

    public bool CanExplore(IHandler handler) => handler is IServiceMethodProvider;

    public async ValueTask ExploreAsync(IRequest request, IHandler handler, List<string> path, OpenApiDocument document, SchemaManager schemata, ApiDiscoveryRegistry registry)
    {
        if (handler is IServiceMethodProvider serviceProvider)
        {
            await registry.ExploreAsync(request, await serviceProvider.GetMethodsAsync(request), path, document, schemata);
        }
    }

}
