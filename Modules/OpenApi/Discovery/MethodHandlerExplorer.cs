using System.Reflection;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Services;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Reflection.Operations;

using NJsonSchema;
using NSwag;

namespace GenHTTP.Modules.OpenApi.Discovery;

public sealed class MethodHandlerExplorer : IApiExplorer
{

    /// <summary>
    /// Describes the content of a request or response body.
    /// </summary>
    /// <param name="Schema">The schema of the content, if known</param>
    /// <param name="MediaTypes">The media types the content can be represented with</param>
    private sealed record BodyDescription(JsonSchema? Schema, string[] MediaTypes);

    public bool CanExplore(IHandler handler) => handler is MethodHandler;

    public ValueTask ExploreAsync(IRequest request, IHandler handler, List<string> path, OpenApiDocument document, SchemaManager schemata, InheritedDocumentation documentation, ApiDiscoveryRegistry registry)
    {
        if (handler is MethodHandler methodHandler)
        {
            var tag = GetTag(methodHandler.Operation);

            if (tag != null)
            {
                AddTag(document, tag);
            }

            var pathItem = OpenApiExtensions.GetPathItem(document, path, methodHandler.Operation);

            var method = methodHandler.Operation.Method;

            foreach (var requestMethod in methodHandler.Operation.Configuration.SupportedMethods)
            {
                if (requestMethod == RequestMethod.Head && methodHandler.Operation.Configuration.SupportedMethods.Count > 1)
                {
                    continue;
                }

                var inherited = documentation.GetDocumentation(requestMethod);

                var operation = new OpenApiOperation
                {
                    Summary = Documentation.GetSummary(method),
                    Description = Documentation.GetRemarks(method),
                    IsDeprecated = method.GetCustomAttributes(typeof(ObsoleteAttribute), true).Length > 0
                };

                if (tag != null)
                {
                    operation.Tags.Add(tag.Name);
                }

                foreach (var arg in methodHandler.Operation.Arguments)
                {
                    if (arg.Value.Source == OperationArgumentSource.Injected)
                    {
                        continue;
                    }

                    var description = GetParameterSummary(method, arg.Key);

                    if (arg.Value.Source == OperationArgumentSource.Body)
                    {
                        if (requestMethod != RequestMethod.Get)
                        {
                            operation.RequestBody = GetRequestBody(new BodyDescription(schemata.GetOrCreateSchema(typeof(string)), ["text/plain"]), description);
                        }
                    }
                    else if (arg.Value.Source == OperationArgumentSource.Content)
                    {
                        if (requestMethod != RequestMethod.Get)
                        {
                            operation.RequestBody = GetRequestBody(new BodyDescription(schemata.GetOrCreateSchema(arg.Value.Type), GetFormats(methodHandler.Registry)), description);
                        }
                    }
                    else if (arg.Value.Source == OperationArgumentSource.Streamed)
                    {
                        if (requestMethod != RequestMethod.Get)
                        {
                            operation.RequestBody = GetRequestBody(new BodyDescription(GetBinarySchema(), ["*/*"]), description);
                        }
                    }
                    else
                    {
                        var param = new OpenApiParameter
                        {
                            Name = arg.Key,
                            Description = description,
                            Schema = JsonSchema.FromType(arg.Value.Type),
                            Kind = MapArgumentType(arg.Value.Source),
                            IsRequired = MapRequired(arg.Value.Source)
                        };

                        operation.Parameters.Add(param);
                    }
                }

                if (requestMethod != RequestMethod.Get)
                {
                    ApplyDeclaredRequestBody(operation, method, schemata, methodHandler.Registry);
                }

                AddRequestHeaders(operation, methodHandler.Operation, inherited);

                if (methodHandler.Operation.Route.IsWildcard)
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

                foreach (var (key, value) in GetResponses(methodHandler.Operation, schemata, methodHandler.Registry, inherited))
                {
                    operation.Responses.Add(key, value);
                }

                AddResponseHeaders(operation, methodHandler.Operation, inherited);

                pathItem.Add(requestMethod.ToString(), operation);
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

    private static string? GetParameterSummary(MethodInfo method, string name)
    {
        var parameter = method.GetParameters().FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));

        return parameter != null ? Documentation.GetSummary(parameter) : null;
    }

    private static void AddRequestHeaders(OpenApiOperation operation, Operation source, IReadOnlyList<OperationDocumentation> inherited)
    {
        foreach (var header in GetDocumentation<RequestHeaderAttribute>(source, inherited))
        {
            if (operation.Parameters.Any(p => p.Kind == OpenApiParameterKind.Header && string.Equals(p.Name, header.Name, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = header.Name,
                Description = header.Description,
                Kind = OpenApiParameterKind.Header,
                IsRequired = header.Required,
                Schema = JsonSchema.FromType(header.Type ?? typeof(string))
            });
        }
    }

    #region Precedence

    /// <summary>
    /// Collects the documentation of the given kind that applies to the operation, the most specific
    /// first: the method, the service and the concerns from the innermost to the outermost.
    /// </summary>
    private static IEnumerable<T> GetDocumentation<T>(Operation operation, IReadOnlyList<OperationDocumentation> inherited) where T : DocumentationAttribute
    {
        foreach (var attribute in operation.Method.GetCustomAttributes<T>(true))
        {
            yield return attribute;
        }

        foreach (var level in GetSharedLevels(operation, inherited))
        {
            foreach (var attribute in level.OfType<T>())
            {
                yield return attribute;
            }
        }
    }

    /// <summary>
    /// The documentation shared by multiple operations (declared on the service or provided by
    /// concerns), grouped by the level it has been declared on, the most specific first.
    /// </summary>
    private static IEnumerable<IEnumerable<DocumentationAttribute>> GetSharedLevels(Operation operation, IReadOnlyList<OperationDocumentation> inherited)
    {
        if (operation.Method.DeclaringType is { } type)
        {
            yield return type.GetCustomAttributes<DocumentationAttribute>(true);
        }

        foreach (var level in inherited)
        {
            yield return level.Entries;
        }
    }

    #endregion

    #region Tags

    private static OpenApiTag? GetTag(Operation operation)
    {
        var method = operation.Method;

        if (method.GetCustomAttribute<TagAttribute>(true) is { } methodTag)
        {
            return new OpenApiTag { Name = methodTag.Name };
        }

        var type = method.DeclaringType;

        if (type == null)
        {
            return null;
        }

        if (type.Name.Contains("<>"))
        {
            return new OpenApiTag { Name = "Inline" };
        }

        return new OpenApiTag
        {
            Name = type.GetCustomAttribute<TagAttribute>(true)?.Name ?? type.Name,
            Description = Documentation.GetDescription(type)
        };
    }

    private static void AddTag(OpenApiDocument document, OpenApiTag tag)
    {
        var existing = document.Tags.FirstOrDefault(t => t.Name == tag.Name);

        if (existing == null)
        {
            document.Tags.Add(tag);
        }
        else if (string.IsNullOrEmpty(existing.Description))
        {
            existing.Description = tag.Description;
        }
    }

    #endregion

    #region Request

    private static OpenApiRequestBody GetRequestBody(BodyDescription body, string? description)
    {
        var requestBody = new OpenApiRequestBody
        {
            Description = description
        };

        foreach (var mediaType in body.MediaTypes)
        {
            requestBody.Content.Add(mediaType, new OpenApiMediaType
            {
                Schema = body.Schema
            });
        }

        return requestBody;
    }

    private static void ApplyDeclaredRequestBody(OpenApiOperation operation, MethodInfo method, SchemaManager schemata, MethodRegistry registry)
    {
        var declarations = method.GetCustomAttributes<RequestBodyAttribute>(true).ToList();

        if (declarations.Count == 0)
        {
            return;
        }

        var inferred = operation.RequestBody;

        var requestBody = new OpenApiRequestBody
        {
            Description = declarations.Select(d => d.Description).FirstOrDefault(d => d != null) ?? inferred?.Description
        };

        foreach (var declaration in declarations)
        {
            var body = GetDeclaredBody(declaration.Type, declaration.ContentType, GetInferredBody(inferred), schemata, registry);

            if (body == null)
            {
                continue;
            }

            foreach (var mediaType in body.MediaTypes)
            {
                requestBody.Content.TryAdd(mediaType, new OpenApiMediaType
                {
                    Schema = body.Schema
                });
            }
        }

        // only the description has been declared, so keep the content as derived from the signature
        if (requestBody.Content.Count == 0 && inferred != null)
        {
            foreach (var (mediaType, content) in inferred.Content)
            {
                requestBody.Content.Add(mediaType, content);
            }
        }

        if (requestBody.Content.Count > 0)
        {
            requestBody.IsRequired = true;
            operation.RequestBody = requestBody;
        }
    }

    private static BodyDescription? GetInferredBody(OpenApiRequestBody? body)
    {
        if (body == null || body.Content.Count == 0)
        {
            return null;
        }

        return new BodyDescription(body.Content.Values.First().Schema, body.Content.Keys.ToArray());
    }

    #endregion

    #region Responses

    private static Dictionary<string, OpenApiResponse> GetResponses(Operation operation, SchemaManager schemata, MethodRegistry registry, IReadOnlyList<OperationDocumentation> inherited)
    {
        var result = new Dictionary<string, OpenApiResponse>();

        var method = operation.Method;

        var resultSummary = Documentation.GetSummary(method.ReturnParameter);

        var inferred = GetResultBody(operation, schemata, registry);

        var declared = method.GetCustomAttributes<ResponseAttribute>(true).ToList();

        if (!declared.Any(d => IsSuccess(d.Status)))
        {
            AddInferredResponses(result, operation, inferred, resultSummary);
        }

        foreach (var group in declared.GroupBy(d => d.Status))
        {
            result[GetKey(group.Key)] = GetDeclaredResponse(group.Key, group, inferred, resultSummary, schemata, registry);
        }

        // responses shared by the service or declared by concerns, the most specific level wins
        foreach (var level in GetSharedLevels(operation, inherited))
        {
            foreach (var group in level.OfType<ResponseAttribute>().GroupBy(d => d.Status))
            {
                result.TryAdd(GetKey(group.Key), GetDeclaredResponse(group.Key, group, null, null, schemata, registry));
            }
        }

        return result;
    }

    private static void AddResponseHeaders(OpenApiOperation operation, Operation source, IReadOnlyList<OperationDocumentation> inherited)
    {
        foreach (var header in GetDocumentation<ResponseHeaderAttribute>(source, inherited))
        {
            List<OpenApiResponse> targets;

            if (header.Status is { } status)
            {
                targets = operation.Responses.TryGetValue(GetKey(status), out var response) ? [response] : [];
            }
            else
            {
                targets = operation.Responses.Where(r => int.TryParse(r.Key, out var code) && code is >= 200 and < 300)
                                             .Select(r => r.Value)
                                             .ToList();
            }

            foreach (var target in targets)
            {
                if (target.Headers.Keys.Any(k => string.Equals(k, header.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                target.Headers.Add(header.Name, new OpenApiHeader
                {
                    Description = header.Description,
                    Schema = JsonSchema.FromType(header.Type ?? typeof(string))
                });
            }
        }
    }

    private static void AddInferredResponses(Dictionary<string, OpenApiResponse> result, Operation operation, BodyDescription? body, string? resultSummary)
    {
        var sink = operation.Result.Sink;

        if (sink == OperationResultSink.None || operation.Result.Type.MightBeNull())
        {
            result.Add("204", new OpenApiResponse
            {
                Description = "A response containing no body"
            });
        }

        if (body != null)
        {
            var response = new OpenApiResponse();

            if (sink is OperationResultSink.Binary or OperationResultSink.Dynamic)
            {
                response.Description = resultSummary ?? "A dynamically generated response";
            }
            else if (resultSummary != null)
            {
                response.Description = resultSummary;
            }

            AddContent(response, body);

            result.Add("200", response);
        }
    }

    private static OpenApiResponse GetDeclaredResponse(ResponseStatus status, IEnumerable<ResponseAttribute> declarations, BodyDescription? inferred, string? resultSummary,
        SchemaManager schemata, MethodRegistry registry)
    {
        var response = new OpenApiResponse();

        string? description = null;

        foreach (var declaration in declarations)
        {
            description ??= declaration.Description;

            var body = GetDeclaredBody(declaration.Type, declaration.ContentType, IsSuccess(status) ? inferred : null, schemata, registry);

            if (body != null)
            {
                AddContent(response, body);
            }
        }

        response.Description = description ?? resultSummary ?? GetDefaultDescription(status);

        return response;
    }

    private static void AddContent(OpenApiResponse response, BodyDescription body)
    {
        foreach (var mediaType in body.MediaTypes)
        {
            response.Content.TryAdd(mediaType, new OpenApiMediaType
            {
                Schema = body.Schema
            });
        }
    }

    /// <summary>
    /// Describes the content generated by the method, as derived from its signature.
    /// </summary>
    private static BodyDescription? GetResultBody(Operation operation, SchemaManager schemata, MethodRegistry registry)
    {
        var type = Unwrap(operation.Result.Type);

        return operation.Result.Sink switch
        {
            OperationResultSink.Formatter => new BodyDescription(schemata.GetOrCreateSchema(type), ["text/plain"]),
            OperationResultSink.Serializer => new BodyDescription(schemata.GetOrCreateSchema(type), GetFormats(registry)),
            OperationResultSink.Binary => new BodyDescription(GetBinarySchema(), ["application/octet-stream"]),
            OperationResultSink.Dynamic => new BodyDescription(null, ["*/*"]),
            _ => null
        };
    }

    /// <summary>
    /// Results wrapped into a <see cref="Result{T}" /> are not unwrapped by the signature
    /// analysis if returned asynchronously (but still serialized, see ResponseProvider).
    /// </summary>
    private static Type Unwrap(Type type)
    {
        if (type.IsGenericType && typeof(IResultWrapper).IsAssignableFrom(type))
        {
            return type.GenericTypeArguments[0];
        }

        return type;
    }

    private static bool IsSuccess(ResponseStatus status) => (int)status is >= 200 and < 300;

    private static string GetKey(ResponseStatus status) => ((int)status).ToString();

    private static string GetDefaultDescription(ResponseStatus status) => status switch
    {
        ResponseStatus.NoContent => "A response containing no body",
        _ => System.Text.RegularExpressions.Regex.Replace(status.ToString(), "(?<=[a-z])(?=[A-Z])", " ")
    };

    #endregion

    #region Bodies

    /// <summary>
    /// Describes the content of a body declared by an attribute, using the body derived from
    /// the method signature to fill the information that has not been declared.
    /// </summary>
    private static BodyDescription? GetDeclaredBody(Type? type, string? contentType, BodyDescription? inferred, SchemaManager schemata, MethodRegistry registry)
    {
        if (type != null)
        {
            var schema = schemata.GetOrCreateSchema(type);

            if (contentType != null)
            {
                return new BodyDescription(schema, [contentType]);
            }

            return new BodyDescription(schema, registry.Formatting.CanHandle(type) ? ["text/plain"] : GetFormats(registry));
        }

        if (inferred != null)
        {
            if (contentType == null)
            {
                return inferred;
            }

            return new BodyDescription(inferred.Schema ?? GuessSchema(contentType), [contentType]);
        }

        return contentType != null ? new BodyDescription(GuessSchema(contentType), [contentType]) : null;
    }

    private static JsonSchema? GuessSchema(string contentType)
    {
        var mediaType = contentType.Split(';')[0].Trim();

        if (mediaType.StartsWith("text/", StringComparison.OrdinalIgnoreCase))
        {
            return new JsonSchema
            {
                Type = JsonObjectType.String
            };
        }

        if (mediaType.EndsWith("json", StringComparison.OrdinalIgnoreCase) || mediaType.EndsWith("xml", StringComparison.OrdinalIgnoreCase) || mediaType.EndsWith("yaml", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return GetBinarySchema();
    }

    private static JsonSchema GetBinarySchema() => new()
    {
        Format = "binary"
    };

    private static string[] GetFormats(MethodRegistry registry) => registry.Serialization.Formats.Select(s => s.Key.ToString()).ToArray();

    #endregion

}
