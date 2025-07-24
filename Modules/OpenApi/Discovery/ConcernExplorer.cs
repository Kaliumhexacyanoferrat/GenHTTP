using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using NSwag;

namespace GenHTTP.Modules.OpenApi.Discovery;

public sealed class ConcernExplorer : IApiExplorer
{

    public bool CanExplore(IHandler handler) => handler is IConcern;

    public async ValueTask ExploreAsync(IRequest request, IHandler handler, List<string> path, OpenApiDocument document, SchemaManager schemata, ApiDiscoveryRegistry registry)
    {
        if (handler is IConcern concern)
        {
            await registry.ExploreAsync(request, concern.Content, path, document, schemata);
        }
    }

}
