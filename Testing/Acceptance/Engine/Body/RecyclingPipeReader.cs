using System.Buffers;
using System.IO.Pipelines;
using System.Text;

namespace GenHTTP.Testing.Acceptance.Engine.Body;

/// <summary>
/// Serves a fixed body in segments of the given size, a few segments per read, and reuses
/// every segment it is advanced past the way a pooling reader may: its bytes are overwritten,
/// its running index is reset and it is unlinked. A reader of the body that still touches
/// memory it has handed back then sees wrong data or a wrong length instead of getting away
/// with it, as it does with a reader that happens not to reuse its segments right away.
/// </summary>
internal sealed class RecyclingPipeReader(byte[] data, int segmentSize, int segmentsPerRead) : PipeReader
{
    private Segment? _head;

    private Segment? _tail;

    private int _headIndex;

    private int _delivered;

    private bool _needsData = true;

    /// <summary>
    /// The number of segments reused after the reader was advanced past them.
    /// </summary>
    public int Recycled { get; private set; }

    public override bool TryRead(out ReadResult result)
    {
        if (_needsData)
        {
            Deliver();
            _needsData = false;
        }

        var buffer = (_head != null && _tail != null) ? new ReadOnlySequence<byte>(_head, _headIndex, _tail, _tail.Memory.Length) : ReadOnlySequence<byte>.Empty;

        result = new ReadResult(buffer, isCanceled: false, isCompleted: _delivered == data.Length);
        return true;
    }

    public override ValueTask<ReadResult> ReadAsync(CancellationToken cancellationToken = default)
    {
        TryRead(out var result);
        return new(result);
    }

    public override void AdvanceTo(SequencePosition consumed) => AdvanceTo(consumed, consumed);

    public override void AdvanceTo(SequencePosition consumed, SequencePosition examined)
    {
        var segment = consumed.GetObject() as Segment;

        if (segment != null)
        {
            var index = consumed.GetInteger();

            // A segment consumed to its end goes as well, when the data goes on in the next one
            var next = segment.Next as Segment;

            if (index == segment.Memory.Length && next != null)
            {
                segment = next;
                index = 0;
            }

            while (_head != null && _head != segment)
            {
                var released = _head;

                _head = released.Next as Segment;

                released.Recycle();
                Recycled++;
            }

            _headIndex = index;
        }

        // More data only once everything buffered has been examined, as a pipe waits for its writer then
        _needsData = examined.GetObject() == _tail && examined.GetInteger() == (_tail?.Memory.Length ?? 0);
    }

    /// <summary>
    /// What is left of the body, once its reader is done with it.
    /// </summary>
    public string Rest()
    {
        TryRead(out var result);

        while (!result.IsCompleted)
        {
            AdvanceTo(result.Buffer.Start, result.Buffer.End);
            TryRead(out result);
        }

        return Encoding.ASCII.GetString(result.Buffer.ToArray());
    }

    public override void CancelPendingRead() { }

    public override void Complete(Exception? exception = null) { }

    private void Deliver()
    {
        for (var i = 0; i < segmentsPerRead && _delivered < data.Length; i++)
        {
            var length = Math.Min(segmentSize, data.Length - _delivered);

            var segment = new Segment(data.AsSpan(_delivered, length).ToArray(), (_tail != null) ? _tail.RunningIndex + _tail.Memory.Length : 0);

            if (_tail == null)
            {
                _head = segment;
                _headIndex = 0;
            }
            else
            {
                _tail.Link(segment);
            }

            _tail = segment;
            _delivered += length;
        }
    }

    private sealed class Segment : ReadOnlySequenceSegment<byte>
    {
        private readonly byte[] _bytes;

        public Segment(byte[] bytes, long runningIndex)
        {
            _bytes = bytes;

            Memory = bytes;
            RunningIndex = runningIndex;
        }

        public void Link(Segment next) => Next = next;

        public void Recycle()
        {
            _bytes.AsSpan().Fill(0xEE);

            RunningIndex = 0;
            Next = null;
        }
    }

}
