using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace GenHTTP.Modules.Mcp.Schema;

internal static class McpSerialization
{

    /// <summary>
    /// Lenient when reading what a model sends (e.g. numbers as strings), compact when writing.
    /// </summary>
    internal static JsonSerializerOptions Default { get; } = ForRuntime(new JsonSerializerOptions(JsonSerializerDefaults.Web)
    {
        RespectNullableAnnotations = true,
        RespectRequiredConstructorParameters = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() }
    });

    internal static JsonSerializerOptions DefaultSchema { get; } = ForSchema(Default);

    /// <summary>
    /// The options to bind arguments with, treating nullable members as optional
    /// so a model does not need to send them.
    /// </summary>
    internal static JsonSerializerOptions ForRuntime(JsonSerializerOptions options)
    {
        options.MakeReadOnly(populateMissingResolver: true);

        var runtimeOptions = new JsonSerializerOptions(options)
        {
            TypeInfoResolver = options.TypeInfoResolver!.WithAddedModifier(OptionalNullables)
        };

        runtimeOptions.MakeReadOnly();

        return runtimeOptions;
    }

    /// <summary>
    /// The options to describe types with, which advertise the strict format
    /// even if the serializer accepts more than that.
    /// </summary>
    internal static JsonSerializerOptions ForSchema(JsonSerializerOptions options)
    {
        var schemaOptions = new JsonSerializerOptions(options)
        {
            NumberHandling = JsonNumberHandling.Strict
        };

        schemaOptions.MakeReadOnly();

        return schemaOptions;
    }

    private static void OptionalNullables(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Kind != JsonTypeInfoKind.Object)
        {
            return;
        }

        foreach (var property in typeInfo.Properties)
        {
            if (property.IsRequired && property.IsSetNullable)
            {
                property.IsRequired = false;
            }
        }
    }

}
