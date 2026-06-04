using System.IO.Pipelines;

namespace GenHTTP.Engine.Shared.Types.Body;

public sealed class StreamConsumptionStrategy
{
    private Stream? _stream;

    private long? _length;

    private PipeReader? _reader;
    
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
        _stream = null;
        _reader = null;
        _length = null;
    }
    
    public Stream Obtain()
    {
        if (_stream != null)
        {
            return _stream;
        }

        var reader = Reader;

        if (_length != null)
        {
            return _stream = new LengthLimitedStream(reader, _length.Value);
        }

        return _stream = new ChunkedBodyStream(reader);
    }
    
    public ValueTask DrainAsync()
    {
        var stream = Obtain();
        
        Reset();
        
        if (stream is IDrainableStream drainable)
        {
            return drainable.DrainAsync();
        }

        return default;
    }
    
    #endregion
    
}
