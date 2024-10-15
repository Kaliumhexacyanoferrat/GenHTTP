using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace GenHTTP.Modules.Reflection.Operations;

public static class SignatureAnalyzer
{

    public static Dictionary<string, OperationArgument> GetArguments(MethodInfo method, HashSet<string> pathArguments, MethodRegistry registry)
    {
        var result = new Dictionary<string, OperationArgument>(StringComparer.OrdinalIgnoreCase);

        foreach (var param in method.GetParameters())
        {
            if (param.Name == null) continue;

            if (pathArguments.Contains(param.Name))
            {
                result.Add(param.Name, new(param.Name, param.ParameterType, OperationArgumentSource.Path));
                continue;
            }

            if (TryInject(param, registry, out var injectedArg))
            {
                result.Add(param.Name, injectedArg);
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
                    result.Add(param.Name, new(param.Name, param.ParameterType, OperationArgumentSource.Query));
                }
            }
            else
            {
                result.Add(param.Name, new(param.Name, param.ParameterType, OperationArgumentSource.Content));
            }
        }

        return result;
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

}
