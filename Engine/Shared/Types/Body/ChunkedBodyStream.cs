using System.Buffers;
using System.IO.Pipelines;

using Glyph11.Parser;

using GlyphParser = Glyph11.Parser.ChunkedBodyStream;

namespace GenHTTP.Engine.Shared.Types.Body;

internal sealed class ChunkedBodyStream : Stream, IDrainableStream
{
    private readonly PipeReader _reader;

    private GlyphParser _parser = new();

    private bool _completed;

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

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (_completed || buffer.IsEmpty)
        {
            return 0;
        }

        if (_overflowLength > 0)
        {
            return ConsumeOverflow(buffer.Span);
        }

        while (true)
        {
            var result = await _reader.ReadAsync(cancellationToken);
            var readBuffer = result.Buffer;

            if (readBuffer.IsEmpty)
            {
                _reader.AdvanceTo(readBuffer.Start, readBuffer.End);

                if (result.IsCompleted)
                {
                    return 0;
                }

                continue;
            }

            var chunkResult = _parser.TryReadChunk(in readBuffer, out var bytesConsumed, out var chunkData);

            switch (chunkResult)
            {
                case ChunkResult.Completed:
                    _reader.AdvanceTo(readBuffer.GetPosition(bytesConsumed));
                    _completed = true;
                    return 0;

                case ChunkResult.NeedMoreData:
                    _reader.AdvanceTo(readBuffer.Start, readBuffer.End);

                    if (result.IsCompleted)
                    {
                        throw new InvalidDataException("Unexpected end of chunked body");
                    }

                    continue;

                case ChunkResult.Chunk:
                    var chunkLength = (int)chunkData.Length;
                    var toCopy = Math.Min(buffer.Length, chunkLength);

                    chunkData.Slice(0, toCopy).CopyTo(buffer.Span);

                    if (toCopy < chunkLength)
                    {
                        _overflow = ArrayPool<byte>.Shared.Rent(chunkLength - toCopy);
                        chunkData.Slice(toCopy).CopyTo(_overflow.AsSpan());
                        _overflowOffset = 0;
                        _overflowLength = chunkLength - toCopy;
                    }

                    _reader.AdvanceTo(readBuffer.GetPosition(bytesConsumed));
                    return toCopy;
            }
        }
    }

    public async ValueTask DrainAsync(CancellationToken cancellationToken = default)
    {
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

            if (buffer.IsEmpty)
            {
                _reader.AdvanceTo(buffer.Start, buffer.End);

                if (result.IsCompleted)
                {
                    return;
                }

                continue;
            }

            var chunkResult = _parser.TryReadChunk(in buffer, out var bytesConsumed, out _);

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
