using System.Text.Json;
using System.Text.Json.Serialization;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Conversion.Serializers.Json;

public sealed class JsonFormat : ISerializationFormat
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    #region Get-/Setters

    public JsonSerializerOptions Options { get; }

    #endregion

    #region Initialization

    public JsonFormat(JsonSerializerOptions? options = null)
    {
        Options = options ?? DefaultOptions;
    }

    #endregion

    #region Functionality

    /// <summary>
    /// Creates a copy of the default serializer settings that can be used
    /// for further customization.
    /// </summary>
    /// <returns>The cloned JSON serializer options</returns>
    public static JsonSerializerOptions GetDefaultOptions() => new(DefaultOptions);

    public ValueTask<object?> DeserializeAsync(Stream stream, Type type) => JsonSerializer.DeserializeAsync(stream, type, Options);

    public ValueTask<IResponseBuilder> SerializeAsync<T>(IRequest request, T response) where T : class
    {
        var result = request.Respond()
                            .Content(new JsonContent<T>(response, Options));

        return new ValueTask<IResponseBuilder>(result);
    }

    public ValueTask<ReadOnlyMemory<byte>> SerializeAsync<T>(T data) where T : class
    {
        return ByteStreamSerialization.SerializeAsync(b =>
        {
            var content = new JsonContent<T>(data, Options);

            return content.WriteAsync(b, 8192);
        });
    }

    #endregion

}
