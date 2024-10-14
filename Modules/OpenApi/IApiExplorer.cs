using GenHTTP.Api.Content;

using GenHTTP.Modules.OpenApi.Discovery;

namespace GenHTTP.Modules.OpenApi;

public interface IApiExplorer
{

    bool CanExplore(IHandler handler);

    void Explore(IHandler handler, List<string> path, ApiDiscoveryRegistry registry);

}
