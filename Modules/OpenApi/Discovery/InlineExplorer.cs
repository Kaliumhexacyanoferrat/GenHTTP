using GenHTTP.Api.Content;
using GenHTTP.Modules.Functional.Provider;
using Microsoft.OpenApi.Models;

namespace GenHTTP.Modules.OpenApi.Discovery;

public class InlineExplorer : IApiExplorer
{

    public bool CanExplore(IHandler handler) => handler is InlineHandler;

    public void Explore(IHandler handler, List<string> path, OpenApiDocument document, ApiDiscoveryRegistry registry)
    {
        if (handler is InlineHandler inlineHandler)
        {
            registry.Explore(inlineHandler, path, document);
        }
    }

}
