using System.Buffers;
using System.Text.Json;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Playground;

public sealed class FixedLengthJsonContent : IResponseContent
{
    private readonly byte[] _buffer;

    public ulong? Length => (ulong)_buffer.Length;

    public FixedLengthJsonContent(JsonResult result)
    {
        using var ms = new MemoryStream(27);
        JsonSerializer.Serialize(ms, result);
        
        ms.TryGetBuffer(out var segment);
        
        _buffer = segment.Array!;
    }

    public ValueTask WriteAsync(IResponseSink sink)
    {
        sink.Writer.Write(_buffer);
        return ValueTask.CompletedTask;
    }
    
}