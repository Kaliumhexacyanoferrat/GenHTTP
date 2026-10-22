using System.Text.Json;

using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Conversion.Serializers;
using GenHTTP.Modules.Conversion.Serializers.Json;

using Namotion.Reflection;
using NJsonSchema;
using NJsonSchema.Generation;
using NSwag;

namespace GenHTTP.Modules.OpenApi.Discovery;

public sealed class SchemaManager
{
    private static readonly JsonSerializerOptions DefaultOptions = JsonFormat.GetDefaultOptions();

    private readonly OpenApiDocument _document;

    private readonly Dictionary<JsonSerializerOptions, JsonSchemaGenerator> _generators = new(ReferenceEqualityComparer.Instance);

    private OpenApiSchemaResolver? _resolver;

    internal SchemaManager(OpenApiDocument document)
    {
        _document = document;
    }

    /// <summary>
    /// Generates or retrieves a JSON schema that represents the given type,
    /// using the default JSON serialization settings of the framework.
    /// </summary>
    /// <param name="type">The type to generate a schema for</param>
    /// <returns>The generated or retrieved JSON schema</returns>
    public JsonSchema GetOrCreateSchema(Type type) => GetOrCreateSchema(type, DefaultOptions);

    /// <summary>
    /// Generates or retrieves a JSON schema that represents the given type
    /// as it will be serialized by the JSON format of the given registry.
    /// </summary>
    /// <param name="type">The type to generate a schema for</param>
    /// <param name="serialization">The registry used to serialize and deserialize instances of the type</param>
    /// <returns>The generated or retrieved JSON schema</returns>
    public JsonSchema GetOrCreateSchema(Type type, SerializationRegistry serialization) => GetOrCreateSchema(type, GetOptions(serialization));

    /// <summary>
    /// Generates or retrieves a JSON schema that represents the given type
    /// as it will be serialized with the given options.
    /// </summary>
    /// <param name="type">The type to generate a schema for</param>
    /// <param name="options">The options used to serialize and deserialize instances of the type</param>
    /// <returns>The generated or retrieved JSON schema</returns>
    /// <remarks>
    /// Schemas are shared by all operations of the document, so a type will be described
    /// with the options it has been encountered with first.
    /// </remarks>
    public JsonSchema GetOrCreateSchema(Type type, JsonSerializerOptions options)
    {
        if (!_generators.TryGetValue(options, out var generator))
        {
            var settings = new SystemTextJsonSchemaGeneratorSettings
            {
                SchemaType = SchemaType.OpenApi3,
                AllowReferencesWithProperties = true,
                SerializerOptions = options
            };

            settings.SchemaProcessors.Add(new DocumentationSchemaProcessor());

            _resolver ??= new OpenApiSchemaResolver(_document, settings);

            generator = new JsonSchemaGenerator(settings);
            _generators.Add(options, generator);
        }

        return generator.GenerateWithReferenceAndNullability<JsonSchema>(type.ToContextualType(), false, _resolver!);
    }

    private static JsonSerializerOptions GetOptions(SerializationRegistry serialization)
    {
        if (serialization.Formats.TryGetValue(ContentType.ApplicationJson, out var preferred) && preferred is JsonFormat preferredJson)
        {
            return preferredJson.Options;
        }

        return serialization.Formats.Values.OfType<JsonFormat>().FirstOrDefault()?.Options ?? DefaultOptions;
    }

}
