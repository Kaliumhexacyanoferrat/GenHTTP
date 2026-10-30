using System.Reflection;
using System.Text.Json;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Mcp.Schema;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Reflection.Operations;

namespace GenHTTP.Modules.Mcp.Provider;

/// <summary>
/// Analyzes a tool method using the reflection framework, so that injection
/// and interceptors work the same way as for webservices.
/// </summary>
internal static class MethodToolFactory
{
    private static readonly ExecutionSettings Settings = new(ExecutionMode.Reflection);

    private static readonly ToolConfiguration Configuration = new();

    internal static RegisteredTool Create(IServer server, MethodToolDefinition definition, MethodRegistry registry, JsonSerializerOptions options, JsonSerializerOptions schemaOptions)
    {
        var method = definition.Method;

        var operation = OperationBuilder.Create(server, null, method, null, Settings, Configuration, registry);

        var parameters = method.GetParameters();

        var bindings = new ToolParameter[parameters.Length];
        var arguments = new List<ToolArgument>(parameters.Length);

        var nullability = new NullabilityInfoContext();

        for (var i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];

            if (parameter.Name == null || !operation.Arguments.TryGetValue(parameter.Name, out var argument))
            {
                throw new InvalidOperationException($"Unable to analyze parameter {i} of tool '{definition.Name}'.");
            }

            if (argument.Source == OperationArgumentSource.Injected)
            {
                bindings[i] = new ToolParameter(argument, null);
            }
            else if (argument.Source == OperationArgumentSource.Streamed)
            {
                throw new InvalidOperationException($"Parameter '{parameter.Name}' of tool '{definition.Name}' is a stream, which cannot be sent by a client.");
            }
            else
            {
                var required = !parameter.HasDefaultValue && !IsNullable(parameter, nullability);

                var toolArgument = new ToolArgument(parameter.Name, parameter, required, argument.DefaultValue, options.GetTypeInfo(parameter.ParameterType), ArgumentValidation.ForParameter(parameter, options));

                bindings[i] = new ToolParameter(argument, toolArgument);
                arguments.Add(toolArgument);
            }
        }

        var outputType = GetOutputType(definition, method.ReturnType);

        var inputSchema = SchemaGenerator.Arguments(arguments, schemaOptions);

        var outputSchema = outputType != null ? SchemaGenerator.Output(outputType, schemaOptions) : null;

        var invoker = new MethodToolInvoker(definition.Instance, operation, registry, bindings, ResultUnwrapping.For(method.ReturnType), options, outputType == null);

        var description = ToolDocumentation.Describe(definition.Description, method, operation);

        return new RegisteredTool(definition.Name, definition.Title, description, definition.Hints, inputSchema, outputSchema, invoker);
    }

    private static bool IsNullable(ParameterInfo parameter, NullabilityInfoContext context)
    {
        if (parameter.ParameterType.IsValueType)
        {
            return Nullable.GetUnderlyingType(parameter.ParameterType) != null;
        }

        return context.Create(parameter).WriteState == NullabilityState.Nullable;
    }

    private static Type? GetOutputType(MethodToolDefinition definition, Type returnType)
    {
        var type = returnType;

        if (type == typeof(void) || type == typeof(Task) || type == typeof(ValueTask))
        {
            return null;
        }

        if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Task<>) || type.GetGenericTypeDefinition() == typeof(ValueTask<>)))
        {
            type = type.GenericTypeArguments[0];
        }

        if (type.IsGenericType && typeof(IResultWrapper).IsAssignableFrom(type))
        {
            type = type.GenericTypeArguments[0];
        }

        if (typeof(IHandler).IsAssignableFrom(type) || typeof(IHandlerBuilder).IsAssignableFrom(type) || typeof(IResponse).IsAssignableFrom(type) || typeof(IResponseBuilder).IsAssignableFrom(type)
            || typeof(Stream).IsAssignableFrom(type) || type == typeof(byte[]) || type == typeof(ReadOnlyMemory<byte>))
        {
            throw new InvalidOperationException($"Tool '{definition.Name}' returns '{type}', which cannot be sent to a client. Please return a string, a ToolResult or a serializable object.");
        }

        return type;
    }

    private sealed class ToolConfiguration : IMethodConfiguration
    {

        public HashSet<RequestMethod> SupportedMethods { get; } = [RequestMethod.Post];

    }

}

/// <summary>
/// A parameter of a tool method, either read from the arguments sent by the client
/// or injected by the framework (if there is no tool argument).
/// </summary>
internal sealed record ToolParameter(OperationArgument Operation, ToolArgument? Argument);
