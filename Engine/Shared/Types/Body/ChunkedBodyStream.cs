using System.Buffers;
using System.IO.Pipelines;

using Glyph11.Parser;

using GlyphParser = Glyph11.Parser.ChunkedBodyStream;

namespace GenHTTP.Engine.Shared.Types.Body;

/// <summary>
/// Decodes an HTTP/1.1 chunked transfer-encoded request body from a <see cref="PipeReader" />,
/// delegating chunk framing to Glyph11's <see cref="GlyphParser" /> struct.
/// </summary>
internal sealed class ChunkedBodyStream : Stream, IDrainableStream
{
    private readonly PipeReader _reader;

    private GlyphParser _parser;

    private bool _completed;

    // Overflow bytes from a chunk that exceeded the caller's ReadAsync destination buffer.
    private byte[]? _overflow;
    private int _overflowOffset;
    private int _overflowLength;

    #region Get-/Setters

    public override bool CanRead => true;

    public override bool CanWrite => false;

    public override bool CanSeek => false;

    public override long Length => throw new NotSupportedException("Length is not known for chunked streams");

    public override long Position
    {
        get => throw new NotSupportedException("Seeking the body stream is not supported");
        set => throw new NotSupportedException("Seeking the body stream is not supported");
    }

    #endregion

    #region Initialization

    public ChunkedBodyStream(PipeReader reader)
    {
        _reader = reader;
    }

    #endregion

    #region Functionality

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        => ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();

    public override async ValueTask<int> ReadAsync(Memory<byte> destination, CancellationToken cancellationToken = default)
    {
        if (_completed || destination.IsEmpty)
        {
            return 0;
        }

        // Return buffered overflow from a previous large chunk first.
        if (_overflowLength > 0)
        {
            return ConsumeOverflow(destination.Span);
        }

        while (true)
        {
            var result = await _reader.ReadAsync(cancellationToken);
            var buffer = result.Buffer;

            byte[]? rented = null;
            ReadOnlySpan<byte> span;

            if (buffer.IsSingleSegment)
            {
                span = buffer.First.Span;
            }
            else
            {
                // Linearise the full multi-segment buffer so Glyph11 gets a contiguous view.
                // Glyph11 requires the complete chunk to be present before returning Chunk;
                // capping would cause perpetual NeedMoreData for chunks larger than the cap.
                var len = (int)buffer.Length;
                rented = ArrayPool<byte>.Shared.Rent(len);
                buffer.CopyTo(rented);
                span = rented.AsSpan(0, len);
            }

            try
            {
                if (span.IsEmpty)
                {
                    _reader.AdvanceTo(buffer.Start, buffer.End);

                    if (result.IsCompleted)
                    {
                        return 0;
                    }

                    continue;
                }

                var chunkResult = _parser.TryReadChunk(span, out var bytesConsumed, out var dataOffset, out var dataLength);

                switch (chunkResult)
                {
                    case ChunkResult.Completed:
                        _reader.AdvanceTo(buffer.GetPosition(bytesConsumed));
                        _completed = true;
                        return 0;

                    case ChunkResult.NeedMoreData:
                        _reader.AdvanceTo(buffer.Start, buffer.End);

                        if (result.IsCompleted)
                        {
                            throw new InvalidDataException("Unexpected end of chunked body");
                        }

                        continue;

                    case ChunkResult.Chunk:
                        var chunk = span.Slice(dataOffset, dataLength);
                        var toCopy = Math.Min(destination.Length, dataLength);

                        // Copy to caller's buffer BEFORE advancing the pipe reader to keep span valid.
                        chunk[..toCopy].CopyTo(destination.Span);

                        if (toCopy < dataLength)
                        {
                            // Stash the excess bytes for the next ReadAsync call.
                            _overflow = ArrayPool<byte>.Shared.Rent(dataLength - toCopy);
                            chunk[toCopy..].CopyTo(_overflow);
                            _overflowOffset = 0;
                            _overflowLength = dataLength - toCopy;
                        }

                        _reader.AdvanceTo(buffer.GetPosition(bytesConsumed));
                        return toCopy;
                }
            }
            finally
            {
                if (rented != null)
                {
                    ArrayPool<byte>.Shared.Return(rented);
                }
            }
        }
    }

    public async ValueTask DrainAsync(CancellationToken cancellationToken = default)
    {
        // Release any buffered overflow — we don't need its contents.
        if (_overflow != null)
        {
            ArrayPool<byte>.Shared.Return(_overflow);
            _overflow = null;
            _overflowOffset = 0;
            _overflowLength = 0;
        }

        while (!_completed)
        {
            var result = await _reader.ReadAsync(cancellationToken);
            var buffer = result.Buffer;

            byte[]? rented = null;
            ReadOnlySpan<byte> span;

            if (buffer.IsSingleSegment)
            {
                span = buffer.First.Span;
            }
            else
            {
                var len = (int)buffer.Length;
                rented = ArrayPool<byte>.Shared.Rent(len);
                buffer.CopyTo(rented);
                span = rented.AsSpan(0, len);
            }

            try
            {
                if (span.IsEmpty)
                {
                    _reader.AdvanceTo(buffer.Start, buffer.End);

                    if (result.IsCompleted)
                    {
                        return;
                    }

                    continue;
                }

                // dataOffset/dataLength are irrelevant — we're discarding payload.
                var chunkResult = _parser.TryReadChunk(span, out var bytesConsumed, out _, out _);

                switch (chunkResult)
                {
                    case ChunkResult.Completed:
                        _reader.AdvanceTo(buffer.GetPosition(bytesConsumed));
                        _completed = true;
                        return;

                    case ChunkResult.NeedMoreData:
                        _reader.AdvanceTo(buffer.Start, buffer.End);

                        if (result.IsCompleted)
                        {
                            return;
                        }

                        break;

                    case ChunkResult.Chunk:
                        _reader.AdvanceTo(buffer.GetPosition(bytesConsumed));
                        break;
                }
            }
            finally
            {
                if (rented != null)
                {
                    ArrayPool<byte>.Shared.Return(rented);
                }
            }
        }
    }

    private int ConsumeOverflow(Span<byte> destination)
    {
        var toCopy = Math.Min(destination.Length, _overflowLength);
        _overflow!.AsSpan(_overflowOffset, toCopy).CopyTo(destination);
        _overflowOffset += toCopy;
        _overflowLength -= toCopy;

        if (_overflowLength == 0)
        {
            ArrayPool<byte>.Shared.Return(_overflow!);
            _overflow = null;
            _overflowOffset = 0;
        }

        return toCopy;
    }

    public override int Read(byte[] buffer, int offset, int count)
        => ReadAsync(buffer.AsMemory(offset, count)).AsTask().GetAwaiter().GetResult();

    public override void Flush()
        => throw new NotSupportedException("Flushing the body stream is not supported");

    public override long Seek(long offset, SeekOrigin origin)
        => throw new NotSupportedException("Seeking the body stream is not supported");

    public override void SetLength(long value)
        => throw new NotSupportedException("Length of the body stream cannot be written to");

    public override void Write(byte[] buffer, int offset, int count)
        => throw new NotSupportedException("Body stream cannot be written to");

    #endregion

}
