using GenHTTP.Api.Content;

using GenHTTP.Modules.Reflection;

using NSwag;

namespace GenHTTP.Modules.OpenApi.Discovery;

public class ServiceExplorer : IApiExplorer
{

    public bool CanExplore(IHandler handler) => handler is IServiceMethodProvider;

    public void Explore(IHandler handler, List<string> path, OpenApiDocument document, ApiDiscoveryRegistry registry)
    {
        if (handler is IServiceMethodProvider serviceProvider)
        {
            registry.Explore(serviceProvider.Methods, path, document);
        }
    }

}
