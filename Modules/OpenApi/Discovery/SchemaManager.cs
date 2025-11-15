using Namotion.Reflection;
using NJsonSchema;
using NJsonSchema.Generation;
using NJsonSchema.NewtonsoftJson.Generation;
using NSwag;

namespace GenHTTP.Modules.OpenApi.Discovery;

public sealed class SchemaManager
{
    private readonly JsonSchemaGenerator _generator;

    private readonly OpenApiSchemaResolver _resolver;

    internal SchemaManager(OpenApiDocument document)
    {
        var settings = new NewtonsoftJsonSchemaGeneratorSettings
        {
            SchemaType = SchemaType.OpenApi3,
            AllowReferencesWithProperties = true
        };

        _generator = new JsonSchemaGenerator(settings);
        _resolver = new OpenApiSchemaResolver(document, settings);
    }

    /// <summary>
    /// Generates or retrieves a JSON schema that represents the given type.
    /// </summary>
    /// <param name="type">The type to generate a schema for</param>
    /// <returns>The generated or retrieved JSON schema</returns>
    public JsonSchema GetOrCreateSchema(Type type) => _generator.GenerateWithReferenceAndNullability<JsonSchema>(type.ToContextualType(), false, _resolver);

}
