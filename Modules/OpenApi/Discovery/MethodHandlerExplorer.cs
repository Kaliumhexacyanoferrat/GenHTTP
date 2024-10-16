using System.Text;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.OpenApi.Handler;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Reflection.Operations;
using NJsonSchema;
using NSwag;

namespace GenHTTP.Modules.OpenApi.Discovery;

public class MethodHandlerExplorer : IApiExplorer
{

    public bool CanExplore(IHandler handler) => handler is MethodHandler;

    public void Explore(IHandler handler, List<string> path, OpenApiDocument document, SchemaManager schemata, ApiDiscoveryRegistry registry)
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

                    if (arg.Value.Source == OperationArgumentSource.Content)
                    {
                        if (method.KnownMethod != RequestMethod.Get)
                        {
                            var supportedTypes = methodHandler.Registry.Serialization.Formats.Select(s => s.Key.RawType).ToArray();
                            operation.RequestBody = GetRequestBody(schemata, arg.Value.Type, supportedTypes);
                        }
                    }
                    else
                    {
                        var param = new OpenApiParameter()
                        {
                            Name = arg.Key,
                            Schema = JsonSchema.FromType(arg.Value.Type),
                            Kind = MapArgumentType(arg.Value.Source),
                            IsRequired = MapRequired(arg.Value.Source)
                        };

                        operation.Parameters.Add(param);
                    }
                }

                foreach (var (key, value) in GetResponses(methodHandler.Operation, schemata, methodHandler.Registry))
                {
                    operation.Responses.Add(key, value);
                }

                pathItem.Add(method.RawMethod, operation);
            }
        }
    }

    private static OpenApiPathItem GetPathItem(OpenApiDocument document, List<string> path, Operation operation)
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

    private static string? GetTag(Operation operation)
    {
        var type = operation.Method.DeclaringType?.Name;

        if (type != null)
        {
            return (type.Contains("<>")) ? "Inline" : type;
        }

        return null;
    }

    private static Dictionary<string, OpenApiResponse> GetResponses(Operation operation, SchemaManager schemata, MethodRegistry registry)
    {
        var result = new Dictionary<string, OpenApiResponse>();

        var sink = operation.Result.Sink;
        var type = operation.Result.Type;

        if (sink == OperationResultSink.None || type.MightBeNull())
        {
            result.Add("204", new OpenApiResponse()
            {
                Description = "A response containing no body"
            });
        }

        if (sink == OperationResultSink.Formatter)
        {
            result.Add("200", GetResponse(schemata, type, "text/plain"));
        }
        else if (sink == OperationResultSink.Serializer)
        {
            result.Add("200", GetResponse(schemata, type, registry.Serialization.Formats.Select(s => s.Key.RawType).ToArray()));
        }
        else if (sink == OperationResultSink.Stream)
        {
            var response = new OpenApiResponse()
            {
                Description = "A dynamically generated response"
            };

            var schema = new JsonSchema()
            {
                Format = "binary"
            };

            response.Content.Add("application/octet-stream", new OpenApiMediaType()
            {
                Schema = schema
            });

            result.Add("200", response);
        }
        else if (sink == OperationResultSink.Dynamic)
        {
            var response = new OpenApiResponse()
            {
                Description = "A dynamically generated response"
            };

            response.Content.Add("*/*", new OpenApiMediaType());

            result.Add("200", response);
        }

        return result;
    }

    private static OpenApiRequestBody GetRequestBody(SchemaManager schemata, Type type, params string[] mediaTypes)
    {
        var requestBody = new OpenApiRequestBody();

        var schema = schemata.GetOrCreateSchema(type);

        foreach (var mediaType in mediaTypes)
        {
            var media = new OpenApiMediaType
            {
                Schema = schema
            };

            requestBody.Content.Add(mediaType, media);
        }

        return requestBody;
    }

    private static OpenApiResponse GetResponse(SchemaManager schemata, Type type, params string[] mediaTypes)
    {
        var response = new OpenApiResponse();

        var schema = schemata.GetOrCreateSchema(type);

        foreach (var mediaType in mediaTypes)
        {
            var media = new OpenApiMediaType
            {
                Schema = schema
            };

            response.Content.Add(mediaType, media);
        }

        return response;
    }

}
