using System.Buffers;
using System.Text.Json;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Conversion.Serializers.Json;

public sealed class JsonContent : IResponseContent
{
    private static readonly JsonWriterOptions WriterOptions = new()
    {
        SkipValidation = true
    };

    // Utf8JsonWriter.Reset() keeps the previous output, so the cached writer is pointed here instead
    private static readonly IBufferWriter<byte> Detached = new ArrayBufferWriter<byte>();

    [ThreadStatic]
    private static Utf8JsonWriter? _cachedWriter;

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
        var writer = RentWriter(sink.Writer);

        try
        {
            JsonSerializer.Serialize(writer, Data, SerializerOptions);
        }
        finally
        {
            ReturnWriter(writer);
        }

        return default;
    }

    private static Utf8JsonWriter RentWriter(IBufferWriter<byte> output)
    {
        var writer = _cachedWriter;

        if (writer == null)
        {
            return new Utf8JsonWriter(output, WriterOptions);
        }

        _cachedWriter = null;

        writer.Reset(output);

        return writer;
    }

    private static void ReturnWriter(Utf8JsonWriter writer)
    {
        writer.Reset(Detached);

        _cachedWriter = writer;
    }

    #endregion

}
