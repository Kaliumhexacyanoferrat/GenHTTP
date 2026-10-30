using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;

using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Modules.Mcp.Schema;

internal static class SchemaGenerator
{
    private static readonly JsonSchemaExporterOptions ExporterOptions = new()
    {
        TreatNullObliviousAsNonNullable = true,
        TransformSchemaNode = Describe
    };

    #region Functionality

    /// <summary>
    /// Describes the arguments of a tool implemented by a method, one property per parameter.
    /// </summary>
    internal static JsonObject Arguments(IEnumerable<ToolArgument> arguments, JsonSerializerOptions options)
    {
        var properties = new JsonObject();
        var required = new JsonArray();

        foreach (var argument in arguments)
        {
            var schema = options.GetJsonSchemaAsNode(argument.Type, ExporterOptions);

            if (schema is JsonObject node)
            {
                var description = FindDescription(argument.Parameter);

                if (description != null)
                {
                    node.Remove("description");
                    node.Insert(0, "description", description);
                }

                if (argument.DefaultValue != null)
                {
                    node["default"] = JsonSerializer.SerializeToNode(argument.DefaultValue, argument.Type, options);
                }

                Constraints.Apply(node, Constraints.Find(argument.Parameter));

                RelaxNullables(node);
            }

            properties[argument.Name] = schema;

            if (argument.Required)
            {
                required.Add(argument.Name);
            }
        }

        var result = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = properties
        };

        if (required.Count > 0)
        {
            result["required"] = required;
        }

        if (properties.Count == 0)
        {
            result["additionalProperties"] = false;
        }

        return result;
    }

    internal static string? FindDescription(ICustomAttributeProvider? provider)
    {
        if (provider == null)
        {
            return null;
        }

        foreach (var attribute in provider.GetCustomAttributes(true))
        {
            if (attribute is DescriptionAttribute description)
            {
                return description.Description;
            }

            if (attribute is SummaryAttribute summary)
            {
                return summary.Text;
            }
        }

        return null;
    }

    internal static JsonObject? Output(Type type, JsonSerializerOptions options)
    {
        if (type == typeof(string) || typeof(ToolResult).IsAssignableFrom(type))
        {
            return null;
        }

        if (options.GetJsonSchemaAsNode(type, ExporterOptions) is JsonObject schema && IsObject(schema) && schema.ContainsKey("properties"))
        {
            schema["type"] = "object";

            // null values are not written, so they cannot be required
            RelaxNullables(schema);

            return schema;
        }

        return null;
    }

    private static bool IsObject(JsonObject schema)
    {
        var type = schema["type"];

        if (type is JsonValue value)
        {
            return value.TryGetValue<string>(out var name) && name == "object";
        }

        if (type is JsonArray types)
        {
            var hasObject = false;

            foreach (var entry in types)
            {
                var name = entry?.GetValue<string>();

                if (name == "object")
                {
                    hasObject = true;
                }
                else if (name != "null")
                {
                    return false;
                }
            }

            return hasObject;
        }

        return false;
    }

    private static JsonNode Describe(JsonSchemaExporterContext context, JsonNode schema)
    {
        if (schema is not JsonObject node)
        {
            return schema;
        }

        var property = context.PropertyInfo;

        if (property != null)
        {
            Constraints.Apply(node, Constraints.Find(property.AttributeProvider, property.AssociatedParameter?.AttributeProvider));
        }

        if (!node.ContainsKey("description"))
        {
            var description = FindDescription(property?.AttributeProvider)
                              ?? FindDescription(property?.AssociatedParameter?.AttributeProvider)
                              ?? FindDescription(property == null ? context.TypeInfo.Type : null);

            if (description != null)
            {
                node.Insert(0, "description", description);
            }
        }

        return node;
    }

    private static void RelaxNullables(JsonNode? node)
    {
        if (node is JsonArray array)
        {
            foreach (var entry in array)
            {
                RelaxNullables(entry);
            }
        }
        else if (node is JsonObject obj)
        {
            if (obj["properties"] is JsonObject properties && obj["required"] is JsonArray required)
            {
                for (var i = required.Count - 1; i >= 0; i--)
                {
                    var name = required[i]?.GetValue<string>();

                    if (name != null && properties[name] is JsonObject property && IsNullable(property))
                    {
                        required.RemoveAt(i);
                    }
                }

                if (required.Count == 0)
                {
                    obj.Remove("required");
                }
            }

            foreach (var (_, child) in obj)
            {
                RelaxNullables(child);
            }
        }
    }

    private static bool IsNullable(JsonObject property)
    {
        if (property["type"] is JsonArray types)
        {
            foreach (var type in types)
            {
                if (type?.GetValue<string>() == "null")
                {
                    return true;
                }
            }
        }

        if (property["enum"] is JsonArray values)
        {
            foreach (var value in values)
            {
                if (value == null)
                {
                    return true;
                }
            }
        }

        return false;
    }

    #endregion

}
