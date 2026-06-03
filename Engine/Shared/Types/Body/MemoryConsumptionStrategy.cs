using System.Buffers;
using System.IO.Pipelines;

namespace GenHTTP.Engine.Shared.Types.Body;

public class MemoryConsumptionStrategy
{
    private long? _length;

    private PipeReader? _reader;

    private ReadResult? _readResult;

    private ReadOnlyMemory<byte>? _memory;

    #region Get-/Setters

    internal PipeReader Reader => _reader ?? throw new InvalidOperationException("Reader has not been initialized");

    #endregion

    #region Functionality

    public void Apply(PipeReader reader, long? length)
    {
        _reader = reader;
        _length = length;
    }

    public void Reset()
    {
        _reader = null;
        _length = null;
    }

    public ValueTask<ReadOnlyMemory<byte>> ObtainAsync()
    {
        if (_memory is not null)
        {
            return new(_memory.Value);
        }
        
        if (_length is not null)
        {
            return ReadLength();
        }

        return ReadChunked();
    }

    private async ValueTask<ReadOnlyMemory<byte>> ReadLength()
    {
        var reader = Reader;

        var length = _length!.Value;
        
        while (true)
        {
            var result = await reader.ReadAsync();

            if (result.Buffer.Length < length)
            {
                reader.AdvanceTo(result.Buffer.Start, result.Buffer.End);
                continue;
            }

            var buffer = result.Buffer.Slice(0, length);

            _readResult = result;

            if (buffer.IsSingleSegment)
            {
                return (_memory = buffer.First).Value;
            }

            var linearized = GC.AllocateUninitializedArray<byte>((int)length);

            buffer.CopyTo(linearized);

            return (_memory = linearized).Value;
        }
    }
    
    private async ValueTask<ReadOnlyMemory<byte>> ReadChunked()
    {
        var chunkedStream = new ChunkedBodyStream(Reader);
        
        var writer = new ArrayBufferWriter<byte>();

        var pool = ArrayPool<byte>.Shared;
        
        var buffer = pool.Rent(16 * 1024);

        try
        {
            int read;

            do
            {
                read = await chunkedStream.ReadAsync(buffer);

                if (read > 0)
                {
                    writer.Write(buffer.AsSpan(0, read));
                }
            }
            while (read > 0);
        }
        finally
        {
            pool.Return(buffer);
        }
        
        return (_memory = writer.WrittenMemory).Value;
    }
    
    public ValueTask DrainAsync()
    {
        if (_readResult is not null)
        {
            Reader.AdvanceTo(_readResult.Value.Buffer.End);
        }

        Reset();

        return default;
    }

    #endregion

}
