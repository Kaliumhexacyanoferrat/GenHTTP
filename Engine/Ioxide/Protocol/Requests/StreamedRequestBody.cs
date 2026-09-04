using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Ioxide.Protocol.Requests;

/// <summary>A request body pulled a chunk at a time off an HTTP/2 or HTTP/3 stream.</summary>
internal sealed class StreamedRequestBody : IRequestBody
{
    private Func<ValueTask<ReadOnlyMemory<byte>>>? _read;

    // Points at one stream's chunk reader. Re-applied per stream, since the body travels with a
    // pooled request that outlives the stream it was last used for.
    internal void Apply(Func<ValueTask<ReadOnlyMemory<byte>>> read) => _read = read;

    // Drops the reader, so a pooled request cannot keep a finished stream alive.
    internal void Reset() => _read = null;

    private Func<ValueTask<ReadOnlyMemory<byte>>> Read
        => _read ?? throw new InvalidOperationException("The request body has not been applied to a stream.");

    // The body as a stream, pulling a chunk at a time rather than buffering it.
    public Stream AsStream() => new PullStream(Read);

    // The whole body at once, which defeats streaming but is what some handlers ask for.
    public async ValueTask<ReadOnlyMemory<byte>> AsMemoryAsync()
    {
        var read = Read;

        var assembled = new MemoryStream();

        while (true)
        {
            var chunk = await read();

            if (chunk.IsEmpty)
            {
                break;
            }

            assembled.Write(chunk.Span);
        }

        return assembled.ToArray();
    }

    /// <summary>The body as a Stream, holding whatever is left of the current chunk.</summary>
    private sealed class PullStream : Stream
    {
        private readonly Func<ValueTask<ReadOnlyMemory<byte>>> _read;

        private ReadOnlyMemory<byte> _current;

        private bool _ended;

        private long _position;

        // Wraps the chunk reader, holding whatever is left of the current chunk.
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

        // Fills from the current chunk, fetching the next one only when it runs out.
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

        // The array overload, over the memory one.
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();

        // A sync read would block the reactor thread on a chunk only that thread can deliver.
        public override int Read(byte[] buffer, int offset, int count)
            => throw new NotSupportedException("The request body must be read asynchronously.");

        // Read-only, so there is nothing to push anywhere.
        public override void Flush() { }

        // A request body arrives once, in order.
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        // The length is whatever the peer sends.
        public override void SetLength(long value) => throw new NotSupportedException();

        // Read-only: this is the client's body, not ours.
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
