using GenHTTP.Api.Content;

using GenHTTP.Modules.OpenApi.Discovery;

using NSwag;

namespace GenHTTP.Modules.OpenApi;

public interface IApiExplorer
{

    bool CanExplore(IHandler handler);

    void Explore(IHandler handler, List<string> path, OpenApiDocument document, ApiDiscoveryRegistry registry);

}
