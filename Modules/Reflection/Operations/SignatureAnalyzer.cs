using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Reflection.Operations;

public static class SignatureAnalyzer
{

    public static Dictionary<string, OperationArgument> GetArguments(IServer server, MethodInfo method, HashSet<string> pathArguments, MethodRegistry registry)
    {
        var result = new Dictionary<string, OperationArgument>(StringComparer.OrdinalIgnoreCase);

        foreach (var param in method.GetParameters())
        {
            var name = param.Name;

            if (name == null)
            {
                continue;
            }

            if (pathArguments.Contains(name))
            {
                result.Add(name, CreateArgument(name, param, OperationArgumentSource.Path));
                continue;
            }

            if (TryInject(server, name, param, registry, out var injectedArg))
            {
                result.Add(name, injectedArg);
                continue;
            }

            if (TryStream(name, param, out var streamedArg))
            {
                result.Add(name, streamedArg);
                continue;
            }

            if (param.CanFormat(registry.Formatting))
            {
                if (TryFromBody(name, param, out var bodyArg))
                {
                    result.Add(name, bodyArg);
                }
                else
                {
                    result.Add(name, CreateArgument(name, param, OperationArgumentSource.Query));
                }
            }
            else
            {
                result.Add(name, CreateArgument(name, param, OperationArgumentSource.Content));
            }
        }

        return result;
    }

    private static OperationArgument CreateArgument(string name, ParameterInfo param, OperationArgumentSource source)
        => new(name, param.ParameterType, source, GetDefaultValue(param));

    private static object? GetDefaultValue(ParameterInfo param)
    {
        if (!param.HasDefaultValue)
        {
            return null;
        }

        var value = param.DefaultValue;

        if (value is null or DBNull or Missing)
        {
            return null;
        }

        // enum defaults are stored as their underlying integral value
        var type = Nullable.GetUnderlyingType(param.ParameterType) ?? param.ParameterType;

        if (type.IsEnum && value.GetType() != type)
        {
            return Enum.ToObject(type, value);
        }

        return value;
    }

    private static bool TryStream(string name, ParameterInfo param, [NotNullWhen(true)] out OperationArgument? argument)
    {
        if (param.ParameterType == typeof(Stream))
        {
            argument = CreateArgument(name, param, OperationArgumentSource.Streamed);
            return true;
        }

        argument = null;
        return false;
    }

    private static bool TryInject(IServer server, string name, ParameterInfo param, MethodRegistry registry, [NotNullWhen(true)] out OperationArgument? argument)
    {
        foreach (var injector in registry.Injection)
        {
            if (injector.Supports(server, param.ParameterType))
            {
                argument = CreateArgument(name, param, OperationArgumentSource.Injected);
                return true;
            }
        }

        argument = null;
        return false;
    }

    private static bool TryFromBody(string name, ParameterInfo param, [NotNullWhen(true)] out OperationArgument? argument)
    {
        var fromBody = param.GetCustomAttribute<FromBodyAttribute>();

        if (fromBody != null)
        {
            argument = CreateArgument(name, param, OperationArgumentSource.Body);
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

        if (typeof(Stream).IsAssignableFrom(type) || type == typeof(byte[]) || type == typeof(ReadOnlyMemory<byte>))
        {
            return new OperationResult(type, OperationResultSink.Binary);
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
        if (type.IsAsync())
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
