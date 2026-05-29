using System.Buffers;
using System.Runtime.CompilerServices;

namespace GenHTTP.Modules.IO.Streaming;

public sealed class WriterStreamAdapter : Stream
{
    private readonly IBufferWriter<byte> _writer;

    #region Get-/Setters

    public override bool CanRead => false;

    public override bool CanSeek => false;

    public override bool CanWrite => true;

    public override long Length => throw new NotSupportedException();

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    #endregion

    #region Initialization

    public WriterStreamAdapter(IBufferWriter<byte> writer)
        => _writer = writer ?? throw new ArgumentNullException(nameof(writer));

    #endregion

    #region Functionality

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void WriteByte(byte value)
    {
        var span = _writer.GetSpan(1);

        span[0] = value;

        _writer.Advance(1);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(buffer);

        Write(buffer.AsSpan(offset, count));
    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        while (!buffer.IsEmpty)
        {
            var destination = _writer.GetSpan(buffer.Length);

            var copyLength = Math.Min(buffer.Length, destination.Length);

            buffer[..copyLength].CopyTo(destination);

            _writer.Advance(copyLength);

            buffer = buffer[copyLength..];
        }
    }

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return ValueTask.FromCanceled(cancellationToken);
        }

        Write(buffer.Span);

        return ValueTask.CompletedTask;
    }

    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled(cancellationToken);
        }

        Write(buffer.AsSpan(offset, count));

        return Task.CompletedTask;
    }

    public override void Flush() { }

    public override Task FlushAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled(cancellationToken);
        }

        return Task.CompletedTask;
    }

    public override int Read(byte[] buffer, int offset, int count)
        => throw new NotSupportedException();

    public override int Read(Span<byte> buffer)
        => throw new NotSupportedException();

    public override ValueTask<int> ReadAsync(
        Memory<byte> buffer,
        CancellationToken cancellationToken = default)
        => ValueTask.FromException<int>(new NotSupportedException());

    public override Task<int> ReadAsync(
        byte[] buffer,
        int offset,
        int count,
        CancellationToken cancellationToken)
        => Task.FromException<int>(new NotSupportedException());

    public override long Seek(long offset, SeekOrigin origin)
        => throw new NotSupportedException();

    public override void SetLength(long value)
        => throw new NotSupportedException();

    #endregion
    
}