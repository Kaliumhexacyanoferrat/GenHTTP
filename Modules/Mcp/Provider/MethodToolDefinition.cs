using System.Reflection;

using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Mcp.Schema;
using GenHTTP.Modules.Reflection;

namespace GenHTTP.Modules.Mcp.Provider;

/// <summary>
/// A tool implemented by a method or a delegate, before it has been analyzed
/// against the server it will run on.
/// </summary>
/// <param name="Instance">Provides the instance to invoke the method on (null for static methods)</param>
/// <param name="ActivatedType">The type to create an instance of, if not provided yet</param>
internal sealed record MethodToolDefinition(string Name, string? Title, string Description, ToolHints? Hints, MethodInfo Method, Func<IRequest, ValueTask<object>>? Instance, Type? ActivatedType = null)
{

    internal static List<MethodToolDefinition> Discover(Type type, Func<IRequest, ValueTask<object>>? instance)
    {
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

        if (methods.Any(m => m.GetCustomAttribute<MethodAttribute>(true) != null))
        {
            throw new InvalidOperationException($"Type '{type}' provides webservice or controller methods and cannot be used to provide tools. Please declare your tools in a dedicated class.");
        }

        var result = new List<MethodToolDefinition>();

        foreach (var method in methods)
        {
            var attribute = method.GetCustomAttribute<ToolAttribute>(true);

            if (attribute == null)
            {
                continue;
            }

            var name = attribute.Name ?? ToolNames.FromMethod(method.Name);

            ToolNames.Validate(name);

            var description = SchemaGenerator.FindDescription(method)
                              ?? throw new InvalidOperationException($"Tool '{name}' ({type}.{method.Name}) needs to be described for the model via a [Summary] or [Description] attribute.");

            var activated = !method.IsStatic && instance == null ? type : null;

            result.Add(new MethodToolDefinition(name, attribute.Title, description, attribute.Hints, method, method.IsStatic ? null : instance, activated));
        }

        if (result.Count == 0)
        {
            throw new InvalidOperationException($"Type '{type}' does not declare any methods annotated with [Tool].");
        }

        return result;
    }

    internal static MethodToolDefinition FromDelegate(string name, string description, Delegate function)
    {
        ToolNames.Validate(name);

        var attribute = function.Method.GetCustomAttribute<ToolAttribute>();

        if (attribute?.Name != null)
        {
            throw new InvalidOperationException($"The name of function tool '{name}' is passed when adding it and must not be specified via the [Tool] attribute.");
        }

        var target = function.Target;

        return new MethodToolDefinition(name, attribute?.Title, description, attribute?.Hints, function.Method, target != null ? _ => new(target) : null);
    }

}
