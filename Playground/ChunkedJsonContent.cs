using System.Text.Json;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Playground;

public sealed class ChunkedJsonContent(JsonResult result) : IResponseContent
{
    private static readonly JsonWriterOptions Options = new()
    {
        SkipValidation = true
    };

    public ulong? Length => null;

    public ValueTask WriteAsync(IResponseSink sink)
    {
        using var writer = new Utf8JsonWriter(sink.Writer, Options);

        JsonSerializer.Serialize(writer, result);
        
        return ValueTask.CompletedTask;
    }
    
}