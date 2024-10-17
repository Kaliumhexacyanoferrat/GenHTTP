using GenHTTP.Api.Content;
using GenHTTP.Modules.OpenApi.Handler;
using NSwag;

namespace GenHTTP.Modules.OpenApi.Discovery;

public class ConcernExplorer : IApiExplorer
{

    public bool CanExplore(IHandler handler) => handler is IConcern;

    public void Explore(IHandler handler, List<string> path, OpenApiDocument document, SchemaManager schemata, ApiDiscoveryRegistry registry)
    {
        if (handler is IConcern concern)
        {
            registry.Explore(concern.Content, path, document, schemata);
        }
    }
}
