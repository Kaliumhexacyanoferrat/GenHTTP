using Namotion.Reflection;
using NJsonSchema;
using NJsonSchema.Generation;
using NJsonSchema.NewtonsoftJson.Generation;
using NSwag;

namespace GenHTTP.Modules.OpenApi.Handler;

public partial class SchemaManager
{
    private readonly JsonSchemaGenerator _Generator;

    private readonly OpenApiSchemaResolver _Resolver;

    public SchemaManager(OpenApiDocument document)
    {
        var settings = new NewtonsoftJsonSchemaGeneratorSettings()
        {
            SchemaType = SchemaType.OpenApi3,
            AllowReferencesWithProperties = true
        };

        _Generator = new JsonSchemaGenerator(settings);
        _Resolver = new OpenApiSchemaResolver(document, settings);
    }

    public JsonSchema GetOrCreateSchema(Type type)
    {
        return _Generator.GenerateWithReferenceAndNullability<JsonSchema>(type.ToContextualType(), false, _Resolver);
    }

}
