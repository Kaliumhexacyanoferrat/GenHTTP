using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Ioxide.Protocol.Requests;

/// <summary>A request body pulled a chunk at a time off an HTTP/2 or HTTP/3 stream.</summary>
internal sealed class StreamedRequestBody : IRequestBody
{
    private readonly Func<ValueTask<ReadOnlyMemory<byte>>> _read;

    // Presents a stream's chunks as a request body the handler chain can read.
    internal StreamedRequestBody(Func<ValueTask<ReadOnlyMemory<byte>>> read)
    {
        _read = read;
    }

    // The body as a stream, pulling a chunk at a time rather than buffering it.
    public Stream AsStream() => new PullStream(_read);

    // The whole body at once, which defeats streaming but is what some handlers ask for.
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
