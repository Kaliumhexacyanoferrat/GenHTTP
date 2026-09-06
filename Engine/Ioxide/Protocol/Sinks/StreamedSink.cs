using System.Buffers;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Ioxide.Protocol.Sinks;

/// <summary>The response sink for an HTTP/2 or HTTP/3 stream, flushing as it writes.</summary>
internal sealed class StreamedSink : IResponseSink
{
    private readonly IBufferWriter<byte> _writer;

    private readonly Func<ValueTask> _flush;

    private Stream? _stream;

    // The response sink for one stream: write into the frame buffer, flush to pace it.
    internal StreamedSink(IBufferWriter<byte> writer, Func<ValueTask> flush)
    {
        _writer = writer;
        _flush = flush;
    }

    public IBufferWriter<byte> Writer => _writer;

    public Stream Stream => _stream ??= new FlushingStream(_writer, _flush);

    /// <summary>A write-only stream over the frame buffer that paces itself with flushes.</summary>
    private sealed class FlushingStream : Stream
    {
        private readonly IBufferWriter<byte> _target;

        private readonly Func<ValueTask> _flush;

        private long _written;

        // A write-only stream over the frame buffer that flushes as it goes.
        internal FlushingStream(IBufferWriter<byte> target, Func<ValueTask> flush)
        {
            _target = target;
            _flush = flush;
        }

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => _written;

        public override long Position
        {
            get => _written;
            set => throw new NotSupportedException();
        }

        // The array overload, over the span one.
        public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));

        // Stages bytes without pacing them: there is no flush on the sync path, so large bodies want async.
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            _target.Write(buffer);
            _written += buffer.Length;
        }

        // Writes and then flushes, which is what lets a large body stream rather than pile up.
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            _target.Write(buffer.Span);
            _written += buffer.Length;

            return _flush();
        }

        // The array overload, over the memory one.
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => WriteAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();

        // A no-op, since only FlushAsync can pace a stream without blocking the reactor.
        public override void Flush() { }

        // Pushes the staged frames out to the peer.
        public override Task FlushAsync(CancellationToken cancellationToken) => _flush().AsTask();

        // Write-only, so reading is a mistake worth reporting.
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        // A response body only goes forwards.
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        // The length is whatever gets written.
        public override void SetLength(long value) => throw new NotSupportedException();
    }
}
