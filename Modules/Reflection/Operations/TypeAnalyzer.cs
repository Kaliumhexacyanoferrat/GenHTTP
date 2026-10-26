using System.Reflection;

namespace GenHTTP.Modules.Reflection.Operations;

public static class TypeAnalyzer
{

    /// <summary>
    /// Finds the public surface of the given API type.
    /// </summary>
    /// <param name="type">The type to analyze</param>
    /// <returns>The public API surface of the given type</returns>
    public static MethodInfo[] FindMethods(Type type)
    {
        return
        [
            .. type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                   .Where(m => m.IsPublic || m.IsAssembly)
        ];
    }

}
