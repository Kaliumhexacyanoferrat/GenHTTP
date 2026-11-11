using System.Buffers;

using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.IO.Streaming;

public sealed class StreamBufferWriter : IBufferWriter<byte>, IDisposable
{
    private static readonly ArrayPool<byte> Pool = ArrayPool<byte>.Shared;

    private readonly Stream _stream;

    private byte[]? _buffer;

    #region Get-/Setters
    
    private byte[] Buffer => _buffer ?? throw new ObjectDisposedException(nameof(StreamBufferWriter));
    
    #endregion
    
    #region Initialization

    /// <summary>
    /// Creates a writer that forwards everything written to it to the given stream.
    /// </summary>
    /// <param name="stream">The stream to write to</param>
    /// <param name="bufferSize">The minimum size of the buffer handed out to callers</param>
    public StreamBufferWriter(Stream stream, int bufferSize = BufferSize.Write)
    {
        _stream = stream;
        _buffer = Pool.Rent(bufferSize);
    }

    #endregion

    #region Functionality

    public void Advance(int count)
    {
        _stream.Write(Buffer, 0, count);
    }

    public Memory<byte> GetMemory(int sizeHint = 0) => Ensure(sizeHint);

    public Span<byte> GetSpan(int sizeHint = 0) => Ensure(sizeHint);

    private byte[] Ensure(int sizeHint)
    {
        var buffer = Buffer;

        if (sizeHint > buffer.Length)
        {
            var larger = Pool.Rent(Math.Max(sizeHint, buffer.Length * 2));

            Pool.Return(buffer);

            _buffer = buffer = larger;
        }

        return buffer;
    }

    #endregion

    #region IDisposable Support

    /// <summary>
    /// Returns the buffer to the pool. The writer cannot be used afterwards.
    /// </summary>
    public void Dispose()
    {
        var buffer = _buffer;

        if (buffer != null)
        {
            _buffer = null;
            Pool.Return(buffer);
        }
    }

    #endregion

}
