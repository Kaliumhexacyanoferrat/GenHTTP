using System.Text;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Reflection.Operations;
using NJsonSchema;
using NSwag;

namespace GenHTTP.Modules.OpenApi.Discovery;

public class MethodHandlerExplorer : IApiExplorer
{

    public bool CanExplore(IHandler handler) => handler is MethodHandler;

    public void Explore(IHandler handler, List<string> path, OpenApiDocument document, ApiDiscoveryRegistry registry)
    {
        if (handler is MethodHandler methodHandler)
        {
            var tag = GetTag(methodHandler.Operation);

            if (tag != null)
            {
                if (!document.Tags.Any(t => t.Name == tag))
                {
                    document.Tags.Add(new OpenApiTag()
                    {
                        Name = tag
                    });
                }
            }

            var pathItem = GetPathItem(document, path, methodHandler.Operation);

            foreach (var method in methodHandler.Configuration.SupportedMethods)
            {
                if ((method == RequestMethod.Head) && methodHandler.Configuration.SupportedMethods.Count > 1) continue;

                var operation = new OpenApiOperation();

                if (tag != null)
                {
                    operation.Tags.Add(tag);
                }

                foreach (var arg in methodHandler.Operation.Arguments)
                {
                    if (arg.Value.Source == OperationArgumentSource.Injected) continue;

                    var param = new OpenApiParameter()
                    {
                        Name = arg.Key,
                        Schema = JsonSchema.FromType(arg.Value.Type),
                        Kind = MapArgumentType(arg.Value.Source),
                        IsRequired = MapRequired(arg.Value.Source)
                    };

                    operation.Parameters.Add(param);
                }

                var response = new OpenApiResponse();

                var media = new OpenApiMediaType();

                media.Schema = JsonSchema.FromType(methodHandler.Operation.Method.ReturnType);

                response.Content.Add("application/json", media);

                // todo: methodHandler.Registry.Formatting.Formatters

                operation.Responses.Add("200", response);

                pathItem.Add(method.RawMethod, operation);
            }
        }
    }

    private OpenApiPathItem GetPathItem(OpenApiDocument document, List<string> path, Operation operation)
    {
        var stringPath = BuildPath(operation.Path.Name, path);

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

    private static OpenApiParameterKind MapArgumentType(OperationArgumentSource source) => source switch
    {
        OperationArgumentSource.Path => OpenApiParameterKind.Path,
        OperationArgumentSource.Body => OpenApiParameterKind.Body,
        OperationArgumentSource.Content => OpenApiParameterKind.ModelBinding,
        OperationArgumentSource.Query => OpenApiParameterKind.Query,
        _ => OpenApiParameterKind.Undefined
    };

    private static bool MapRequired(OperationArgumentSource source) => source switch
    {
        OperationArgumentSource.Path => true,
        OperationArgumentSource.Content => true,
        _ => false
    };

    private string? GetTag(Operation operation)
    {
        var type = operation.Method.DeclaringType?.Name;

        if (type != null)
        {
            return (type.Contains("<>")) ? "Inline" : type;
        }

        return null;
    }

}
