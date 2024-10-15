using System.Text;
using GenHTTP.Api.Content;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Reflection.Operations;

using Microsoft.OpenApi.Models;

namespace GenHTTP.Modules.OpenApi.Discovery;

public class MethodHandlerExplorer : IApiExplorer
{

    public bool CanExplore(IHandler handler) => handler is MethodHandler;

    public void Explore(IHandler handler, List<string> path, OpenApiDocument document, ApiDiscoveryRegistry registry)
    {
        if (handler is MethodHandler methodHandler)
        {
            var pathItem = GetPathItem(document, path, methodHandler.Operation);
        }
    }

    private OpenApiPathItem GetPathItem(OpenApiDocument document, List<string> path, Operation operation)
    {
        var stringPath = BuildPath(operation.Path.Name, path);

        document.Paths ??= new();

        if (document.Paths.TryGetValue(stringPath, out var existing))
        {
            return existing;
        }

        var newPath = new OpenApiPathItem();

        document.Paths.Add(stringPath, newPath);

        return newPath;
    }

    private static string BuildPath(string name, List<string> pathParts)
    {
        var builder = new StringBuilder("/");

        if (pathParts.Count > 0)
        {
            builder.Append(string.Join('/', pathParts));
            builder.Append('/');
        }

        if (name.Length > 0 && name[0] == '/')
        {
            builder.Append(name[1..]);
        }
        else
        {
            builder.Append(name);
        }

        return builder.ToString();
    }

}
