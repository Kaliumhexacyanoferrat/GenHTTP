using GenHTTP.Api.Content;

using GenHTTP.Modules.Reflection;

using Microsoft.OpenApi.Models;

namespace GenHTTP.Modules.OpenApi.Discovery;

public class MethodHandlerExplorer : IApiExplorer
{

    public bool CanExplore(IHandler handler) => handler is MethodHandler;

    public void Explore(IHandler handler, List<string> path, OpenApiDocument document, ApiDiscoveryRegistry registry)
    {
        if (handler is MethodHandler methodHandler)
        {
            var pathItem = GetPathItem(document, path, methodHandler.Routing);
        }
    }

    private OpenApiPathItem GetPathItem(OpenApiDocument document, List<string> path, MethodRouting route)
    {
        var stringPath = $"/{string.Join('/', path)}/{route.ParsedPath}";

        if (document.Paths.TryGetValue(stringPath, out var existing))
        {
            return existing;
        }

        var newPath = new OpenApiPathItem();

        document.Paths.Add(stringPath, newPath);

        return newPath;
    }

}
