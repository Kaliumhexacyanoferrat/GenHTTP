using System.Buffers;
using System.IO.Pipelines;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using Glyph11;
using Glyph11.Parser;

using ChunkParser = Glyph11.Parser.ChunkedBodyStream;

namespace GenHTTP.Engine.Shared.Types.Body;

/// <summary>
/// A body sent with <c>Transfer-Encoding: chunked</c>.
/// </summary>
/// <remarks>
/// A decoded chunk stays in the buffer of the reader until it has been
/// handed out completely, so reading with small buffers does not need
/// to copy the remainder of a chunk anywhere else.
/// </remarks>
internal sealed class ChunkedFraming : IBodyFraming
{
    private PipeReader? _reader;

    private ChunkParser _parser;

    private bool _completed;

    private long _position;

    private ReadOnlyMemory<byte> _segment;

    private ReadOnlySequence<byte> _remaining;

    private SequencePosition? _chunkEnd;

    #region Get-/Setters

    public long? Length => null;

    public long Position => _position;

    private PipeReader Reader => _reader ?? throw new InvalidOperationException("Framing has not been initialized");

    #endregion

    #region Initialization

    internal void Apply(PipeReader reader)
    {
        _reader = reader;
        _parser = new();
        _completed = false;
        _position = 0;
        _segment = default;
        _remaining = default;
        _chunkEnd = null;
    }

    public void Reset()
    {
        _reader = null;
        _parser = new();
        _completed = false;
        _position = 0;
        _segment = default;
        _remaining = default;
        _chunkEnd = null;
    }

    #endregion

    #region Functionality

    public ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken)
    {
        if (buffer.IsEmpty)
        {
            return new(0);
        }

        if (_chunkEnd != null)
        {
            return new(CopyFromChunk(buffer.Span));
        }

        return ReadNextAsync(buffer, cancellationToken);
    }

    private async ValueTask<int> ReadNextAsync(Memory<byte> buffer, CancellationToken cancellationToken)
    {
        var reader = Reader;

        try
        {
            while (!_completed)
            {
                var result = await reader.ReadAsync(cancellationToken);
                var available = result.Buffer;

                if (!available.IsEmpty)
                {
                    switch (_parser.TryReadChunk(in available, out var consumed, out var data))
                    {
                        case ChunkResult.Chunk:
                            if (data.Length <= buffer.Length)
                            {
                                return CopyChunk(data, available.GetPosition(consumed), buffer.Span);
                            }

                            HoldChunk(data, available.GetPosition(consumed));
                            return CopyFromChunk(buffer.Span);

                        case ChunkResult.Completed:
                            reader.AdvanceTo(available.GetPosition(consumed));
                            _completed = true;
                            return 0;
                    }
                }

                reader.AdvanceTo(available.Start, available.End);

                if (result.IsCompleted)
                {
                    throw UnexpectedEnd();
                }
            }

            return 0;
        }
        catch (HttpParseException e)
        {
            throw Malformed(e);
        }
    }

    public async ValueTask<ReadOnlyMemory<byte>> ReadToEndAsync(CancellationToken cancellationToken)
    {
        var writer = new ArrayBufferWriter<byte>();

        if (_chunkEnd != null)
        {
            WriteChunk(writer);
        }

        await ConsumeAsync(writer, cancellationToken);

        return writer.WrittenMemory;
    }

    public ValueTask<bool> SkipAsync()
    {
        ReleaseChunk();

        return _completed ? new(true) : SkipRemainingAsync();
    }

    private async ValueTask<bool> SkipRemainingAsync()
    {
        try
        {
            await ConsumeAsync(null, CancellationToken.None);
            return true;
        }
        catch (ProviderException)
        {
            return false;
        }
    }

    /// <summary>
    /// Reads the remaining chunks, passing their data to the given writer (if any).
    /// </summary>
    private async ValueTask ConsumeAsync(ArrayBufferWriter<byte>? writer, CancellationToken cancellationToken)
    {
        var reader = Reader;

        while (!_completed)
        {
            var result = await reader.ReadAsync(cancellationToken);
            var available = result.Buffer;

            var consumed = ConsumeBuffered(available, writer);

            if (_completed)
            {
                reader.AdvanceTo(consumed);
                return;
            }

            reader.AdvanceTo(consumed, available.End);

            if (result.IsCompleted)
            {
                throw UnexpectedEnd();
            }
        }
    }

    /// <summary>
    /// Parses all complete chunks of the given buffer, so that the reader
    /// needs to be advanced only once.
    /// </summary>
    /// <returns>The position up to which the buffer has been consumed</returns>
    private SequencePosition ConsumeBuffered(ReadOnlySequence<byte> available, ArrayBufferWriter<byte>? writer)
    {
        var rest = available;

        try
        {
            while (!rest.IsEmpty)
            {
                var status = _parser.TryReadChunk(in rest, out var consumed, out var data);

                if (status == ChunkResult.NeedMoreData)
                {
                    break;
                }

                if (status == ChunkResult.Completed)
                {
                    _completed = true;
                    return rest.GetPosition(consumed);
                }

                if (writer != null)
                {
                    foreach (var segment in data)
                    {
                        writer.Write(segment.Span);
                    }

                    _position += data.Length;
                }

                rest = rest.Slice(consumed);
            }

            return rest.Start;
        }
        catch (HttpParseException e)
        {
            throw Malformed(e);
        }
    }

    private static ProviderException UnexpectedEnd() => new(ResponseStatus.BadRequest, "Unexpected end of chunked body");

    private static ProviderException Malformed(HttpParseException e) => new((ResponseStatus)e.StatusCode, "Failed to read chunked encoded body", e);

    private int CopyChunk(ReadOnlySequence<byte> data, SequencePosition end, Span<byte> destination)
    {
        data.CopyTo(destination);

        Reader.AdvanceTo(end);

        var length = (int)data.Length;

        _position += length;

        return length;
    }

    private void HoldChunk(ReadOnlySequence<byte> data, SequencePosition end)
    {
        _chunkEnd = end;

        if (data.IsSingleSegment)
        {
            _segment = data.First;
            _remaining = default;
        }
        else
        {
            _remaining = data;
            NextSegment();
        }
    }

    private void NextSegment()
    {
        do
        {
            _segment = _remaining.First;
            _remaining = _remaining.Slice(_segment.Length);
        }
        while (_segment.IsEmpty && !_remaining.IsEmpty);
    }

    private int CopyFromChunk(Span<byte> destination)
    {
        var copied = 0;

        while (true)
        {
            var count = Math.Min(destination.Length - copied, _segment.Length);

            _segment.Span[..count].CopyTo(destination[copied..]);
            _segment = _segment[count..];

            copied += count;

            if (_segment.IsEmpty)
            {
                if (_remaining.IsEmpty)
                {
                    ReleaseChunk();
                    break;
                }

                NextSegment();
            }

            if (copied == destination.Length)
            {
                break;
            }
        }

        _position += copied;

        return copied;
    }

    private void WriteChunk(ArrayBufferWriter<byte> writer)
    {
        writer.Write(_segment.Span);

        _position += _segment.Length;

        foreach (var segment in _remaining)
        {
            writer.Write(segment.Span);

            _position += segment.Length;
        }

        ReleaseChunk();
    }

    private void ReleaseChunk()
    {
        if (_chunkEnd != null)
        {
            Reader.AdvanceTo(_chunkEnd.Value);

            _segment = default;
            _remaining = default;
            _chunkEnd = null;
        }
    }

    #endregion

}
