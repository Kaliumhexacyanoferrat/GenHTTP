using System.Buffers;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.InternalH3Experimental.Protocol;

/// <summary>
/// The sink a response body writes into.
/// </summary>
internal sealed class H3Sink : IResponseSink
{
    private readonly ArrayBufferWriter<byte> _writer;

    internal H3Sink(ArrayBufferWriter<byte> writer)
    {
        _writer = writer;
    }

    public IBufferWriter<byte> Writer => _writer;

    public Stream Stream => _stream ??= new BufferWriterStream(_writer);

    private Stream? _stream;

    // Content that writes to a Stream rather than an IBufferWriter, which most of the IO module
    // does.
    private sealed class BufferWriterStream : Stream
    {
        private readonly IBufferWriter<byte> _target;

        internal BufferWriterStream(IBufferWriter<byte> target) => _target = target;

        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));

        public override void Write(ReadOnlySpan<byte> buffer) => _target.Write(buffer);

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            _target.Write(buffer.Span);
            return new ValueTask();
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            _target.Write(buffer.AsSpan(offset, count));
            return Task.CompletedTask;
        }

        public override void Flush() { }
        public override Task FlushAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
    }
}
