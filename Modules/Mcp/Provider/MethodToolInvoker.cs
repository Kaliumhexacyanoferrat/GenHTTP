using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Mcp.Schema;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Reflection.Operations;

namespace GenHTTP.Modules.Mcp.Provider;

/// <summary>
/// Invokes a tool method: reads its arguments, injects framework values,
/// runs the interceptors and captures the result.
/// </summary>
internal sealed class MethodToolInvoker
{
    private readonly Func<IRequest, ValueTask<object>>? _instance;

    private readonly MethodInvoker _invoker;

    private readonly Operation _operation;

    private readonly MethodRegistry _registry;

    private readonly ToolParameter[] _parameters;

    private readonly Func<object?, ValueTask<object?>> _unwrap;

    private readonly JsonTypeInfo<object?> _output;

    private readonly bool _isVoid;

    #region Initialization

    internal MethodToolInvoker(Func<IRequest, ValueTask<object>>? instance, Operation operation, MethodRegistry registry, ToolParameter[] parameters, Func<object?, ValueTask<object?>> unwrap, JsonSerializerOptions options, bool isVoid)
    {
        _instance = instance;
        _operation = operation;
        _registry = registry;
        _parameters = parameters;
        _unwrap = unwrap;
        _isVoid = isVoid;

        _invoker = MethodInvoker.Create(operation.Method);
        _output = (JsonTypeInfo<object?>)options.GetTypeInfo(typeof(object));
    }

    #endregion

    #region Functionality

    public async ValueTask<ToolOutcome> InvokeAsync(JsonElement arguments, ToolContext context)
    {
        var values = new object?[_parameters.Length];

        var named = _operation.Interceptors.Count > 0 ? new Dictionary<ByteString, object?>(_parameters.Length) : null;

        List<string>? invalid = null;

        try
        {
            for (var i = 0; i < _parameters.Length; i++)
            {
                var parameter = _parameters[i];

                object? value;

                var argument = parameter.Argument;

                if (argument != null)
                {
                    var problem = Read(arguments, argument, out value);

                    if (problem != null)
                    {
                        return ToolOutcome.Failed(problem);
                    }

                    argument.Validator?.Validate(value, argument.Name, invalid ??= []);
                }
                else
                {
                    value = await ArgumentProvider.GetInjectedArgumentAsync(context.Request, context.Handler, parameter.Operation, _registry);
                }

                values[i] = value;

                if (named != null)
                {
                    named[parameter.Operation.Name] = value;
                }
            }

            if (invalid?.Count > 0)
            {
                return ToolOutcome.Failed(ArgumentValidation.Format(invalid));
            }

            if (named != null)
            {
                foreach (var interceptor in _operation.Interceptors)
                {
                    var interception = await interceptor.InterceptAsync(context.Request, _operation, named);

                    if (interception != null)
                    {
                        return ToolOutputs.Capture(interception.Payload, _output);
                    }
                }
            }

            var target = _instance != null ? await _instance(context.Request) : null;

            var output = await _unwrap(_invoker.Invoke(target, values.AsSpan()));

            if (output is IResultWrapper wrapper)
            {
                output = wrapper.Payload;
            }

            return _isVoid ? ToolOutputs.Empty() : ToolOutputs.Capture(output, _output);
        }
        catch (Exception e)
        {
            return ToolOutputs.Fail(e);
        }
    }

    private static string? Read(JsonElement arguments, ToolArgument argument, out object? value)
    {
        if (!arguments.TryGetProperty(argument.EncodedName, out var element) || element.ValueKind == JsonValueKind.Null)
        {
            value = argument.DefaultValue;

            return argument.Required ? $"The argument '{argument.Name}' is required." : null;
        }

        try
        {
            value = element.Deserialize(argument.TypeInfo);
            return null;
        }
        catch (JsonException e)
        {
            value = null;
            return $"The argument '{argument.Name}' does not match its schema: {e.Message}";
        }
    }

    #endregion

}

/// <summary>
/// Awaits the result of a tool method, if needed.
/// </summary>
internal static class ResultUnwrapping
{
    private static readonly MethodInfo AwaitTaskMethod = typeof(ResultUnwrapping).GetMethod(nameof(AwaitTaskOf), BindingFlags.NonPublic | BindingFlags.Static)!;

    private static readonly MethodInfo AwaitValueTaskMethod = typeof(ResultUnwrapping).GetMethod(nameof(AwaitValueTaskOf), BindingFlags.NonPublic | BindingFlags.Static)!;

    internal static Func<object?, ValueTask<object?>> For(Type returnType)
    {
        if (returnType == typeof(Task))
        {
            return AwaitTask;
        }

        if (returnType == typeof(ValueTask))
        {
            return AwaitValueTask;
        }

        if (returnType.IsGenericType)
        {
            var definition = returnType.GetGenericTypeDefinition();

            if (definition == typeof(Task<>))
            {
                return AwaitTaskMethod.MakeGenericMethod(returnType.GenericTypeArguments).CreateDelegate<Func<object?, ValueTask<object?>>>();
            }

            if (definition == typeof(ValueTask<>))
            {
                return AwaitValueTaskMethod.MakeGenericMethod(returnType.GenericTypeArguments).CreateDelegate<Func<object?, ValueTask<object?>>>();
            }
        }

        return Identity;
    }

    private static ValueTask<object?> Identity(object? result) => new(result);

    private static async ValueTask<object?> AwaitTask(object? result)
    {
        await (Task)result!;
        return null;
    }

    private static async ValueTask<object?> AwaitValueTask(object? result)
    {
        await (ValueTask)result!;
        return null;
    }

    private static async ValueTask<object?> AwaitTaskOf<T>(object? result) => await (Task<T>)result!;

    private static async ValueTask<object?> AwaitValueTaskOf<T>(object? result) => await (ValueTask<T>)result!;

}
