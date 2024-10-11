using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.Conversion.Serializers.Json;

public sealed class JsonFormat : ISerializationFormat
{
    private static readonly JsonSerializerOptions OPTIONS = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public ValueTask<object?> DeserializeAsync(Stream stream, Type type)
    {
            return JsonSerializer.DeserializeAsync(stream, type, OPTIONS);
        }

    public ValueTask<IResponseBuilder> SerializeAsync(IRequest request, object response)
    {
            var result = request.Respond()
                                .Content(new JsonContent(response, OPTIONS))
                                .Type(ContentType.ApplicationJson);

            return new ValueTask<IResponseBuilder>(result);
        }

}
