using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Ioxide.Protocol.Multiplexed;

internal sealed class MultiplexedRequestBody : IRequestBody
{
    private readonly Func<ValueTask<ReadOnlyMemory<byte>>> _read;

    internal MultiplexedRequestBody(Func<ValueTask<ReadOnlyMemory<byte>>> read)
    {
        _read = read;
    }

    public Stream AsStream() => new PullStream(_read);

    public async ValueTask<ReadOnlyMemory<byte>> AsMemoryAsync()
    {
        var assembled = new MemoryStream();

        while (true)
        {
            var chunk = await _read();

            if (chunk.IsEmpty)
            {
                break;
            }

            assembled.Write(chunk.Span);
        }

        return assembled.ToArray();
    }

    private sealed class PullStream : Stream
    {
        private readonly Func<ValueTask<ReadOnlyMemory<byte>>> _read;

        private ReadOnlyMemory<byte> _current;

        private bool _ended;

        private long _position;

        internal PullStream(Func<ValueTask<ReadOnlyMemory<byte>>> read)
        {
            _read = read;
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => _position;
            set => throw new NotSupportedException();
        }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (_current.IsEmpty && !_ended)
            {
                _current = await _read();

                if (_current.IsEmpty)
                {
                    _ended = true;
                }
            }

            if (_current.IsEmpty)
            {
                return 0;
            }

            var take = Math.Min(buffer.Length, _current.Length);

            _current[..take].CopyTo(buffer);
            _current = _current[take..];
            _position += take;

            return take;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => await ReadAsync(buffer.AsMemory(offset, count), cancellationToken);

        public override int Read(byte[] buffer, int offset, int count)
            => throw new NotSupportedException("The request body must be read asynchronously.");

        public override void Flush() { }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
