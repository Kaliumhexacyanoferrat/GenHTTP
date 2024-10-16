using System.Reflection;
using GenHTTP.Modules.Conversion.Formatters;

namespace GenHTTP.Modules.Reflection;

public static class Extensions
{
    private static readonly Type? VoidTaskResult = Type.GetType("System.Threading.Tasks.VoidTaskResult");

    /// <summary>
    /// Checks, whether the given parameter can be passed via the URL.
    /// </summary>
    /// <param name="info">The parameter to be analyzes</param>
    /// <returns><c>true</c>, if the given parameter can be passed via the URL</returns>
    public static bool CanFormat(this ParameterInfo info, FormatterRegistry formatters) => info.CheckNullable() || formatters.CanHandle(info.ParameterType);

    /// <summary>
    /// Checks, whether the given parameter is a nullable value type.
    /// </summary>
    /// <param name="info">The parameter to be analyzes</param>
    /// <returns><c>true</c>, if the given parameter is a nullable value type</returns>
    public static bool CheckNullable(this ParameterInfo info) => Nullable.GetUnderlyingType(info.ParameterType) is not null;

    /// <summary>
    /// Creates a regular expression that will match the name of the
    /// given parameter.
    /// </summary>
    /// <param name="name">The name to generate the expression from</param>
    /// <returns>The newly created expression</returns>
    public static string ToParameter(this string name) => $"(?<{name}>[^/]+)";

    public static bool IsAsyncGeneric(this Type resultType) => resultType.IsAssignableToGenericType(typeof(ValueTask<>)) || resultType.IsAssignableToGenericType(typeof(Task<>));

    public static bool IsGenericallyVoid(this Type type) => type.GenericTypeArguments.Length == 1 && type.GenericTypeArguments[0] == VoidTaskResult;

    public static bool IsAssignableToGenericType(this Type givenType, Type genericType)
    {
        var interfaceTypes = givenType.GetInterfaces();

        foreach (var it in interfaceTypes)
        {
            if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
            {
                return true;
            }
        }

        if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
        {
            return true;
        }

        var baseType = givenType.BaseType;

        if (baseType == null)
        {
            return false;
        }

        return IsAssignableToGenericType(baseType, genericType);
    }

}
