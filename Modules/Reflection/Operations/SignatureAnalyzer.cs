using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Reflection.Operations;

public static class SignatureAnalyzer
{

    public static Dictionary<string, OperationArgument> GetArguments(MethodInfo method, HashSet<string> pathArguments, MethodRegistry registry)
    {
        var result = new Dictionary<string, OperationArgument>(StringComparer.OrdinalIgnoreCase);

        foreach (var param in method.GetParameters())
        {
            if (param.Name == null)
            {
                continue;
            }

            if (pathArguments.Contains(param.Name))
            {
                result.Add(param.Name, new OperationArgument(param.Name, param.ParameterType, OperationArgumentSource.Path));
                continue;
            }

            if (TryInject(param, registry, out var injectedArg))
            {
                result.Add(param.Name, injectedArg);
                continue;
            }

            if (TryStream(param, out var streamedArg))
            {
                result.Add(param.Name, streamedArg);
                continue;
            }

            if (param.CanFormat(registry.Formatting))
            {
                if (TryFromBody(param, out var bodyArg))
                {
                    result.Add(param.Name, bodyArg);
                }
                else
                {
                    result.Add(param.Name, new OperationArgument(param.Name, param.ParameterType, OperationArgumentSource.Query));
                }
            }
            else
            {
                result.Add(param.Name, new OperationArgument(param.Name, param.ParameterType, OperationArgumentSource.Content));
            }
        }

        return result;
    }

    private static bool TryStream(ParameterInfo param, [NotNullWhen(true)] out OperationArgument? argument)
    {
        if (param.ParameterType == typeof(Stream))
        {
            argument = new OperationArgument(param.Name!, param.ParameterType, OperationArgumentSource.Streamed);
            return true;
        }

        argument = null;
        return false;
    }

    private static bool TryInject(ParameterInfo param, MethodRegistry registry, [NotNullWhen(true)] out OperationArgument? argument)
    {
        foreach (var injector in registry.Injection)
        {
            if (injector.Supports(param.ParameterType))
            {
                argument = new OperationArgument(param.Name!, param.ParameterType, OperationArgumentSource.Injected);
                return true;
            }
        }

        argument = null;
        return false;
    }

    private static bool TryFromBody(ParameterInfo param, [NotNullWhen(true)] out OperationArgument? argument)
    {
        var fromBody = param.GetCustomAttribute<FromBodyAttribute>();

        if (fromBody != null)
        {
            argument = new OperationArgument(param.Name!, param.ParameterType, OperationArgumentSource.Body);
            return true;
        }

        argument = null;
        return false;
    }

    public static OperationResult GetResult(MethodInfo method, MethodRegistry registry)
    {
        var type = FindActualType(method);

        if (type == null || type.FullName == "System.Void")
        {
            return new OperationResult(method.ReturnType, OperationResultSink.None);
        }

        if (typeof(IHandler).IsAssignableFrom(type) || typeof(IHandlerBuilder).IsAssignableFrom(type) || typeof(IResponse).IsAssignableFrom(type) || typeof(IResponseBuilder).IsAssignableFrom(type))
        {
            return new OperationResult(type, OperationResultSink.Dynamic);
        }

        if (typeof(Stream).IsAssignableFrom(type))
        {
            return new OperationResult(type, OperationResultSink.Stream);
        }

        if (registry.Formatting.CanHandle(type))
        {
            return new OperationResult(type, OperationResultSink.Formatter);
        }

        return new OperationResult(type, OperationResultSink.Serializer);
    }

    private static Type? FindActualType(MethodInfo method)
    {
        var type = method.ReturnType;

        if (type.IsAsyncGeneric())
        {
            return type.IsGenericallyVoid() ? null : type.GenericTypeArguments[0];
        }
        if (type == typeof(ValueTask) || type == typeof(Task))
        {
            return null;
        }

        if (typeof(IResultWrapper).IsAssignableFrom(type))
        {
            return type.GenericTypeArguments[0];
        }

        return type;
    }
}
