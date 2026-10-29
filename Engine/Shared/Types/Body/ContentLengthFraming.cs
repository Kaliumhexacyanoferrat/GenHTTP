using System.Buffers;
using System.IO.Pipelines;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Shared.Types.Body;

/// <summary>
/// A body framed by a <c>Content-Length</c> header.
/// </summary>
internal sealed class ContentLengthFraming : IBodyFraming
{
    private PipeReader? _reader;

    private long _length;

    private long _remaining;

    private SequencePosition? _heldEnd;

    #region Get-/Setters

    public long? Length => _length;

    public long Position => _length - _remaining;

    private PipeReader Reader => _reader ?? throw new InvalidOperationException("Framing has not been initialized");

    #endregion

    #region Initialization

    internal void Apply(PipeReader reader, long length)
    {
        _reader = reader;
        _length = length;
        _remaining = length;
        _heldEnd = null;
    }

    public void Reset()
    {
        _reader = null;
        _length = 0;
        _remaining = 0;
        _heldEnd = null;
    }

    #endregion

    #region Functionality

    public async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken)
    {
        if (_remaining == 0 || buffer.IsEmpty)
        {
            return 0;
        }

        var reader = Reader;

        while (true)
        {
            var result = await reader.ReadAsync(cancellationToken);
            var available = result.Buffer;

            if (!available.IsEmpty)
            {
                var count = (int)Math.Min(Math.Min(buffer.Length, _remaining), available.Length);

                available.Slice(0, count).CopyTo(buffer.Span);

                reader.AdvanceTo(available.GetPosition(count));

                _remaining -= count;

                return count;
            }

            reader.AdvanceTo(available.Start, available.End);

            if (result.IsCompleted)
            {
                throw UnexpectedEnd();
            }
        }
    }

    public ValueTask<ReadOnlyMemory<byte>> ReadToEndAsync(CancellationToken cancellationToken)
        => (_remaining == 0) ? new(ReadOnlyMemory<byte>.Empty) : ReadRemainingAsync(cancellationToken);

    private async ValueTask<ReadOnlyMemory<byte>> ReadRemainingAsync(CancellationToken cancellationToken)
    {
        var reader = Reader;

        while (true)
        {
            var result = await reader.ReadAsync(cancellationToken);
            var available = result.Buffer;

            if (available.Length >= _remaining)
            {
                var body = available.Slice(0, _remaining);

                _remaining = 0;

                if (body.IsSingleSegment)
                {
                    // the view points into the buffer of the reader, so it is not
                    // advanced before the body is skipped
                    _heldEnd = body.End;
                    return body.First;
                }

                var linearized = GC.AllocateUninitializedArray<byte>((int)body.Length);

                body.CopyTo(linearized);

                reader.AdvanceTo(body.End);

                return linearized;
            }

            reader.AdvanceTo(available.Start, available.End);

            if (result.IsCompleted)
            {
                throw UnexpectedEnd();
            }
        }
    }

    public ValueTask<bool> SkipAsync()
    {
        if (_heldEnd != null)
        {
            Reader.AdvanceTo(_heldEnd.Value);
            _heldEnd = null;
        }

        return (_remaining == 0) ? new(true) : SkipRemainingAsync();
    }

    private async ValueTask<bool> SkipRemainingAsync()
    {
        var reader = Reader;

        while (_remaining > 0)
        {
            var result = await reader.ReadAsync();
            var available = result.Buffer;

            var count = Math.Min(_remaining, available.Length);

            reader.AdvanceTo(available.GetPosition(count));

            _remaining -= count;

            if (_remaining > 0 && result.IsCompleted)
            {
                return false;
            }
        }

        return true;
    }

    private static ProviderException UnexpectedEnd() => new(ResponseStatus.BadRequest, "Unexpected end of body");

    #endregion

}
