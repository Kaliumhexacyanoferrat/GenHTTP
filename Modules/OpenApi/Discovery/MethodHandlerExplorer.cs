using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Reflection.Operations;
using NJsonSchema;
using NSwag;

namespace GenHTTP.Modules.OpenApi.Discovery;

public sealed class MethodHandlerExplorer : IApiExplorer
{

    public bool CanExplore(IHandler handler) => handler is MethodHandler;

    public ValueTask ExploreAsync(IRequest request, IHandler handler, List<string> path, OpenApiDocument document, SchemaManager schemata, ApiDiscoveryRegistry registry)
    {
        if (handler is MethodHandler methodHandler)
        {
            var tag = GetTag(methodHandler.Operation);

            if (tag != null)
            {
                if (document.Tags.All(t => t.Name != tag))
                {
                    document.Tags.Add(new OpenApiTag
                    {
                        Name = tag
                    });
                }
            }

            var pathItem = OpenApiExtensions.GetPathItem(document, path, methodHandler.Operation);

            foreach (var method in methodHandler.Configuration.SupportedMethods)
            {
                if (method == RequestMethod.Head && methodHandler.Configuration.SupportedMethods.Count > 1)
                {
                    continue;
                }

                var operation = new OpenApiOperation
                {
                    IsDeprecated = methodHandler.Operation.Method.GetCustomAttributes(typeof(ObsoleteAttribute), true).Length > 0
                };

                if (tag != null)
                {
                    operation.Tags.Add(tag);
                }

                foreach (var arg in methodHandler.Operation.Arguments)
                {
                    if (arg.Value.Source == OperationArgumentSource.Injected)
                    {
                        continue;
                    }

                    if (arg.Value.Source == OperationArgumentSource.Body)
                    {
                        if (method.KnownMethod != RequestMethod.Get)
                        {
                            operation.RequestBody = GetRequestBody(schemata, typeof(string), "text/plain");
                        }
                    }
                    else if (arg.Value.Source == OperationArgumentSource.Content)
                    {
                        if (method.KnownMethod != RequestMethod.Get)
                        {
                            var supportedTypes = methodHandler.Registry.Serialization.Formats.Select(s => s.Key.RawType).ToArray();
                            operation.RequestBody = GetRequestBody(schemata, arg.Value.Type, supportedTypes);
                        }
                    }
                    else if (arg.Value.Source == OperationArgumentSource.Streamed)
                    {
                        if (method.KnownMethod != RequestMethod.Get)
                        {
                            var body = new OpenApiRequestBody();

                            body.Content.Add("*/*", new OpenApiMediaType
                            {
                                Schema = new JsonSchema
                                {
                                    Format = "binary"
                                }
                            });

                            operation.RequestBody = body;
                        }
                    }
                    else
                    {
                        var param = new OpenApiParameter
                        {
                            Name = arg.Key,
                            Schema = JsonSchema.FromType(arg.Value.Type),
                            Kind = MapArgumentType(arg.Value.Source),
                            IsRequired = MapRequired(arg.Value.Source)
                        };

                        operation.Parameters.Add(param);
                    }
                }

                if (methodHandler.Operation.Path.IsWildcard)
                {
                    var param = new OpenApiParameter
                    {
                        Name = "remainingPath",
                        Description = "Additional path segments to be handled by this operation",
                        Kind = OpenApiParameterKind.Path,
                        Schema = JsonSchema.FromType<string?>(),
                        IsRequired = true
                    };

                    operation.Parameters.Add(param);
                }

                foreach (var (key, value) in GetResponses(methodHandler.Operation, schemata, methodHandler.Registry))
                {
                    operation.Responses.Add(key, value);
                }

                pathItem.Add(method.RawMethod, operation);
            }
        }

        return ValueTask.CompletedTask;
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
            return type.Contains("<>") ? "Inline" : type;
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
            result.Add("204", new OpenApiResponse
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
            var response = new OpenApiResponse
            {
                Description = "A dynamically generated response"
            };

            var schema = new JsonSchema
            {
                Format = "binary"
            };

            response.Content.Add("application/octet-stream", new OpenApiMediaType
            {
                Schema = schema
            });

            result.Add("200", response);
        }
        else if (sink == OperationResultSink.Dynamic)
        {
            var response = new OpenApiResponse
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
