using System.Text;
using GenHTTP.Modules.Reflection.Operations;
using NSwag;

namespace GenHTTP.Modules.OpenApi.Discovery;

public static class OpenApiExtensions
{

    public static bool MightBeNull(this Type type)
    {
        if (type.IsClass)
        {
            return true;
        }

        return Nullable.GetUnderlyingType(type) != null;
    }

    public static OpenApiPathItem GetPathItem(OpenApiDocument document, List<string> path, Operation operation)
    {
        var stringPath = BuildPath(operation.Path.Name, path, operation.Path.IsWildcard);

        if (document.Paths.TryGetValue(stringPath, out var existing))
        {
            return existing;
        }

        var newPath = new OpenApiPathItem();

        document.Paths.Add(stringPath, newPath);

        return newPath;
    }

    public static string BuildPath(string name, List<string> pathParts, bool wildcard)
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

        if (wildcard)
        {
            builder.Append("{remainingPath}");
        }

        return builder.ToString();
    }

}
