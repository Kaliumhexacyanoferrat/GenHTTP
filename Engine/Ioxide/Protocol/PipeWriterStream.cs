using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;

namespace GenHTTP.Engine.Ioxide.Protocol;

/// <summary>
/// Minimal write-only <see cref="Stream"/> over an <see cref="IBufferWriter{T}"/> so a response
/// sink's Stream writes through the SAME buffer writer as everything else, preserving ordering.
/// <paramref name="sink"/> receives the bytes (the raw <see cref="PipeWriter"/> for fixed-length
/// responses, or a <see cref="ChunkedWriter"/> for chunked ones); <paramref name="flush"/> is the
/// underlying pipe that actually drains to the socket. Mirrors GenHTTP's WritingStream.
/// </summary>
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

    // Deliberately a no-op: a synchronous flush would block the reactor thread on the pipe's
    // IValueTaskSource, but that flush is completed by the very same reactor -> deadlock. The bytes
    // written above are already buffered in `sink` and get drained by the end-of-response FlushAsync
    // (and async callers can await FlushAsync below), so dropping the sync flush loses nothing.
    public override void Flush() { }

    public override Task FlushAsync(CancellationToken cancellationToken) => flush.FlushAsync(cancellationToken).AsTask();

    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();
}
