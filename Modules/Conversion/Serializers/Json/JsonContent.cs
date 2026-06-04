using System.Text.Json;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Conversion.Serializers.Json;

public sealed class JsonContent : IResponseContent
{
    private readonly JsonWriterOptions _writerOptions = new()
    {
        SkipValidation = true
    };

    #region Get-/Setters

    public ulong? Length => null;

    public ContentType? Type => ContentType.ApplicationJson;

    public ReadOnlyMemory<byte>? Encoding => null;

    private object Data { get; }

    private JsonSerializerOptions SerializerOptions { get; }

    #endregion
    
    #region Initialization

    public JsonContent(object data, JsonSerializerOptions serializerOptions)
    {
        Data = data;
        SerializerOptions = serializerOptions;
    }

    #endregion

    #region Functionality

    public ValueTask<ulong?> CalculateChecksumAsync() => new((ulong)Data.GetHashCode());

    public async ValueTask WriteAsync(Stream target, uint bufferSize)
    {
        await JsonSerializer.SerializeAsync(target, Data, Data.GetType(), SerializerOptions);
    }

    public ValueTask WriteAsync(IResponseSink sink)
    {
        using var writer = new Utf8JsonWriter(sink.Writer, _writerOptions);

        JsonSerializer.Serialize(writer, Data, SerializerOptions);

        return default;
    }

    #endregion

}
