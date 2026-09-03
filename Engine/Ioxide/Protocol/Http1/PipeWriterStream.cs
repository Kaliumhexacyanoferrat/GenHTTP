using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;

namespace GenHTTP.Engine.Ioxide.Protocol.Http1;

internal sealed class PipeWriterStream(IBufferWriter<byte> sink, PipeWriter flush) : Stream
{
    public override bool CanRead => false;

    public override bool CanSeek => false;

    public override bool CanWrite => true;

    public override long Length => throw new NotSupportedException();

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Write(ReadOnlySpan<byte> buffer)
    {
        var destination = sink.GetSpan(buffer.Length);
        buffer.CopyTo(destination);
        sink.Advance(buffer.Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void WriteByte(byte value)
    {
        var span = sink.GetSpan(1);
        span[0] = value;
        sink.Advance(1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        Write(buffer.AsSpan(offset, count));
        return Task.CompletedTask;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        Write(buffer.Span);
        return ValueTask.CompletedTask;
    }

    // No-op on purpose: a sync flush would block the reactor thread on a pipe only that
    // reactor completes. The bytes drain at the end-of-response FlushAsync.
    public override void Flush() { }

    public override Task FlushAsync(CancellationToken cancellationToken) => flush.FlushAsync(cancellationToken).AsTask();

    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();
}
