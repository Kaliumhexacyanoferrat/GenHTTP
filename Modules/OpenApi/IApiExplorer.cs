using GenHTTP.Api.Content;

using GenHTTP.Modules.OpenApi.Discovery;
using Microsoft.OpenApi.Models;

namespace GenHTTP.Modules.OpenApi;

public interface IApiExplorer
{

    bool CanExplore(IHandler handler);

    void Explore(IHandler handler, List<string> path, OpenApiDocument document, ApiDiscoveryRegistry registry);

}
