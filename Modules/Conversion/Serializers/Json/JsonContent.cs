using System.Text.Json;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Conversion.Serializers.Json;

public sealed class JsonContent : IResponseContent
{
    private static readonly ReadOnlyMemory<byte> ContentType = "application/json"u8.ToArray();

    #region Initialization

    public JsonContent(object data, JsonSerializerOptions options)
    {
        Data = data;
        Options = options;
    }

    #endregion

    #region Get-/Setters

    public ulong? Length => null;

    public ReadOnlyMemory<byte> Type => ContentType;

    private object Data { get; }

    private JsonSerializerOptions Options { get; }

    #endregion

    #region Functionality

    public ValueTask<ulong?> CalculateChecksumAsync() => new((ulong)Data.GetHashCode());

    public async ValueTask WriteAsync(Stream target, uint bufferSize)
    {
        await JsonSerializer.SerializeAsync(target, Data, Data.GetType(), Options);
    }

    public ValueTask WriteAsync(IResponseSink sink)
    {
        using var writer = new Utf8JsonWriter(sink.Writer);
        
        JsonSerializer.Serialize(writer, Data, Options);
        
        return default;
    }
    
    #endregion

}
