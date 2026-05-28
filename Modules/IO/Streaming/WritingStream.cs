using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;

namespace GenHTTP.Modules.IO.Streaming;

public sealed class WritingStream : Stream
{
    private readonly Stream _baseStream;
    
    private readonly IBufferWriter<byte> _writer;
    
    private readonly PipeWriter _flusher;

    #region Get-/Setters
    
    public override bool CanRead => _baseStream.CanRead;

    public override bool CanSeek => _baseStream.CanSeek;

    public override bool CanWrite => true;

    public override long Length => _baseStream.Length;

    public override long Position
    {
        get => _baseStream.Position;
        set => _baseStream.Position = value;
    }
    
    #endregion
    
    #region Initialization
    
    public WritingStream(PipeWriter writer, Stream readingStream)
        : this(writer, writer, readingStream) { }

    public WritingStream(IBufferWriter<byte> writer, PipeWriter flusher, Stream baseStream)
    {
        _baseStream = baseStream;
        _writer = writer;
        _flusher = flusher;
    }
    
    #endregion
    
    #region Functionality
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int Read(byte[] buffer, int offset, int count)
        => _baseStream.Read(buffer, offset, count);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int Read(Span<byte> buffer)
        => _baseStream.Read(buffer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int ReadByte()
        => _baseStream.ReadByte();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        => _baseStream.ReadAsync(buffer, offset, count, cancellationToken);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        return _baseStream.ReadAsync(buffer, cancellationToken);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Write(byte[] buffer, int offset, int count)
        => Write(buffer.AsSpan(offset, count));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Write(ReadOnlySpan<byte> buffer)
    {
        var destination = _writer.GetSpan(buffer.Length);

        buffer.CopyTo(destination);

        _writer.Advance(buffer.Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void WriteByte(byte value)
    {
        var span = _writer.GetSpan(1);

        span[0] = value;

        _writer.Advance(1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        Write(buffer, offset, count);

        return Task.CompletedTask;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        Write(buffer.Span);

        return ValueTask.CompletedTask;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Flush()
        => _flusher.FlushAsync().GetAwaiter().GetResult();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override Task FlushAsync(CancellationToken cancellationToken)
        => _flusher.FlushAsync(cancellationToken).AsTask();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override long Seek(long offset, SeekOrigin origin)
        => _baseStream.Seek(offset, origin);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void SetLength(long value)
        => _baseStream.SetLength(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        => _baseStream.CopyToAsync(destination, bufferSize, cancellationToken);

    #endregion
    
}