using System.Reflection;

using NJsonSchema.Generation;

namespace GenHTTP.Modules.OpenApi.Discovery;

/// <summary>
/// Adds the documentation declared on model types and their
/// properties to the generated JSON schemas.
/// </summary>
/// <remarks>
/// For positional records, the documentation may also be placed
/// on the parameters of the primary constructor.
/// </remarks>
internal sealed class DocumentationSchemaProcessor : ISchemaProcessor
{

    public void Process(SchemaProcessorContext context)
    {
        var type = context.ContextualType.Type;
        var schema = context.Schema;

        if (Documentation.GetDescription(type) is { } typeDescription)
        {
            schema.Description = typeDescription;
        }

        if (schema.Properties.Count == 0)
        {
            return;
        }

        var parameters = GetConstructorParameters(type);

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var description = Documentation.GetDescription(property);

            if (description == null && parameters.TryGetValue(property.Name, out var parameter))
            {
                description = Documentation.GetSummary(parameter);
            }

            if (description == null)
            {
                continue;
            }

            var key = schema.Properties.Keys.FirstOrDefault(k => string.Equals(k, property.Name, StringComparison.OrdinalIgnoreCase));

            if (key != null)
            {
                schema.Properties[key].Description = description;
            }
        }
    }

    private static Dictionary<string, ParameterInfo> GetConstructorParameters(Type type)
    {
        var result = new Dictionary<string, ParameterInfo>(StringComparer.Ordinal);

        foreach (var constructor in type.GetConstructors())
        {
            foreach (var parameter in constructor.GetParameters())
            {
                if (parameter.Name != null)
                {
                    result.TryAdd(parameter.Name, parameter);
                }
            }
        }

        return result;
    }

}
