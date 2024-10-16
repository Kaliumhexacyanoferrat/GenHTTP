using GenHTTP.Api.Content;

using GenHTTP.Modules.OpenApi.Discovery;
using GenHTTP.Modules.OpenApi.Handler;
using NSwag;

namespace GenHTTP.Modules.OpenApi;

public interface IApiExplorer
{

    bool CanExplore(IHandler handler);

    void Explore(IHandler handler, List<string> path, OpenApiDocument document, SchemaManager schemata, ApiDiscoveryRegistry registry);

}
