using System.Buffers;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Ioxide.Protocol.Multiplexed;

/// <summary>
/// Writes response content straight into a protocol response writer, which is itself an
/// <see cref="IBufferWriter{T}"/> - so the buffer channel reaches the wire with nothing between.
/// </summary>
/// <remarks>
/// The stream channel flushes on every write, and that await is the backpressure that bounds a
/// large download. The buffer channel flushes once at the end - fine for a page, which is why file
/// content should use the stream.
/// </remarks>
internal sealed class MultiplexedSink : IResponseSink
{
    private readonly IBufferWriter<byte> _writer;

    private readonly Func<ValueTask> _flush;

    private Stream? _stream;

    internal MultiplexedSink(IBufferWriter<byte> writer, Func<ValueTask> flush)
    {
        _writer = writer;
        _flush = flush;
    }

    public IBufferWriter<byte> Writer => _writer;

    public Stream Stream => _stream ??= new FlushingStream(_writer, _flush);

    /// <summary>
    /// Adapts the protocol writer to the stream channel, flushing each write so the peer paces it.
    /// </summary>
    private sealed class FlushingStream : Stream
    {
        private readonly IBufferWriter<byte> _target;

        private readonly Func<ValueTask> _flush;

        private long _written;

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

        public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            // Staged, but not paced: there is no flush on the sync path. Large bodies want async.
            _target.Write(buffer);
            _written += buffer.Length;
        }

        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            _target.Write(buffer.Span);
            _written += buffer.Length;

            await _flush();
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => await WriteAsync(buffer.AsMemory(offset, count), cancellationToken);

        public override void Flush() { }

        public override async Task FlushAsync(CancellationToken cancellationToken) => await _flush();

        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();
    }
}
