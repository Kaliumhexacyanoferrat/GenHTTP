using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;

namespace GenHTTP.Engine.Ioxide.Protocol.Sinks;

/// <summary>A write-only stream over a pipe, for content that only knows how to write to a Stream.</summary>
// Not Shared's WritingStream: its Flush blocks on FlushAsync, and on a reactor thread that
// waits for a pipe only that same thread completes.
internal sealed class Http1WriterStream(IBufferWriter<byte> sink, PipeWriter flush) : Stream
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
    // Copies straight into the pipe's own buffer, so nothing is staged in between.
    public override void Write(ReadOnlySpan<byte> buffer)
    {
        var destination = sink.GetSpan(buffer.Length);
        buffer.CopyTo(destination);
        sink.Advance(buffer.Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // The array overload, over the span one.
    public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // One byte, without going through a span the caller has to allocate.
    public override void WriteByte(byte value)
    {
        var span = sink.GetSpan(1);
        span[0] = value;
        sink.Advance(1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // Already synchronous: the write is a copy, so there is nothing to await.
    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        Write(buffer.AsSpan(offset, count));
        return Task.CompletedTask;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // The memory overload, likewise finishing before it returns.
    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        Write(buffer.Span);
        return ValueTask.CompletedTask;
    }

    // No-op on purpose: a sync flush would block the reactor thread on a pipe only that
    // reactor completes. The bytes drain at the end-of-response FlushAsync.
    public override void Flush() { }

    // The real flush: pushes what is buffered out to the connection.
    public override Task FlushAsync(CancellationToken cancellationToken) => flush.FlushAsync(cancellationToken).AsTask();

    // Write-only, so reading is a mistake worth reporting.
    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    // A response body only goes forwards.
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    // The length is whatever gets written, and it cannot be declared in advance.
    public override void SetLength(long value) => throw new NotSupportedException();
}
