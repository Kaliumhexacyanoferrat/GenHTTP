using System.Buffers;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Ioxide.Protocol.Mux;

/// <summary>
/// Writes response content straight into a protocol response writer.
/// </summary>
/// <remarks>
/// Both protocol writers are themselves <see cref="IBufferWriter{T}"/>, so content that writes
/// through the buffer channel goes to the wire with nothing in between.
///
/// <para>The stream channel flushes on every write, which is what makes a large download bounded:
/// a flush parks until the peer's window and the connection's send retention allow more, so the
/// await is the backpressure. Content that writes through the buffer channel instead is flushed
/// once, when it finishes - fine for a page, and the reason file content should use the stream.</para>
/// </remarks>
internal sealed class MuxSink : IResponseSink
{
    private readonly IBufferWriter<byte> _writer;

    private readonly Func<ValueTask> _flush;

    private Stream? _stream;

    internal MuxSink(IBufferWriter<byte> writer, Func<ValueTask> flush)
    {
        _writer = writer;
        _flush = flush;
    }

    public IBufferWriter<byte> Writer => _writer;

    public Stream Stream => _stream ??= new FlushingStream(_writer, _flush);

    /// <summary>
    /// Adapts the protocol writer to the stream channel, flushing each write so the content is
    /// paced by the peer rather than accumulated.
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
            // Synchronous write: the bytes are staged, but the flush that paces them cannot happen
            // here. Content that writes large bodies should use the async path.
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
