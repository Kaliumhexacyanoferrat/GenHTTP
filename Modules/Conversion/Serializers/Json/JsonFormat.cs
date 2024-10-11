using System.Text.Json;
using System.Text.Json.Serialization;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.Conversion.Serializers.Json;

public sealed class JsonFormat : ISerializationFormat
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public ValueTask<object?> DeserializeAsync(Stream stream, Type type) => JsonSerializer.DeserializeAsync(stream, type, Options);

    public ValueTask<IResponseBuilder> SerializeAsync(IRequest request, object response)
    {
        var result = request.Respond()
                            .Content(new JsonContent(response, Options))
                            .Type(ContentType.ApplicationJson);

        return new ValueTask<IResponseBuilder>(result);
    }
}
