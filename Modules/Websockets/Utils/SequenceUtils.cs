using System.Buffers;
using GenHTTP.Modules.Websockets.Provider;

namespace GenHTTP.Modules.Websockets.Utils;

public static class SequenceUtils
{
    public static ReadOnlySequence<byte> ConcatSequences(List<ReadOnlySequence<byte>?>? sequences)
    {
        if (sequences is null || sequences.Count == 0)
            return ReadOnlySequence<byte>.Empty;

        BufferSegment? first = null;
        BufferSegment? last  = null;

        // for-loop is slightly faster than foreach on List<T>
        for (var i = 0; i < sequences.Count; i++)
        {
            var seqNullable = sequences[i];
            if (!seqNullable.HasValue)
                continue;

            var seq = seqNullable.Value;

            foreach (var mem in seq)
            {
                if (mem.Length == 0)
                    continue;

                if (first is null)
                {
                    first = last = new BufferSegment(mem);
                }
                else
                {
                    last = last!.Append(mem);
                }
            }
        }

        return first is null
            ? ReadOnlySequence<byte>.Empty
            : new ReadOnlySequence<byte>(first, 0, last!, last!.Memory.Length);
    }
    
    public static ReadOnlySequence<byte> ConcatSequences(ReadOnlySequence<byte>[] sequences)
    {
        BufferSegment? first = null;
        BufferSegment? last = null;

        foreach (var seq in sequences)
        {
            foreach (var mem in seq) // mem is ReadOnlyMemory<byte> for each segment
            {
                if (mem.Length == 0) continue;

                if (first is null)
                    first = last = new BufferSegment(mem);
                else
                    last = last!.Append(mem);
            }
        }

        if (first is null)
            return ReadOnlySequence<byte>.Empty;

        return new ReadOnlySequence<byte>(first, 0, last!, last!.Memory.Length);
    }
    
    // Hot path: concat only 2
    public static ReadOnlySequence<byte> ConcatTwo(in ReadOnlySequence<byte> a, in ReadOnlySequence<byte> b)
    {
        if (a.IsEmpty) return b;
        if (b.IsEmpty) return a;

        // Fast path: both single-segment.
        if (a.IsSingleSegment && b.IsSingleSegment)
        {
            var head = new BufferSegment(a.First);
            var tail = head.Append(b.First);
            return new ReadOnlySequence<byte>(head, 0, tail, tail.Memory.Length);
        }

        BufferSegment? headSeg = null;
        BufferSegment? tailSeg = null;

        AppendSequence(a, ref headSeg, ref tailSeg);
        AppendSequence(b, ref headSeg, ref tailSeg);

        if (headSeg is null) return ReadOnlySequence<byte>.Empty;

        return new ReadOnlySequence<byte>(headSeg, 0, tailSeg!, tailSeg!.Memory.Length);
    }

    private static void AppendSequence(in ReadOnlySequence<byte> seq, ref BufferSegment? head, ref BufferSegment? tail)
    {
        // foreach over ReadOnlySequence<byte> uses the struct enumerator; no allocations.
        foreach (var mem in seq)
        {
            if (mem.Length == 0) continue;

            if (head is null)
            {
                head = tail = new BufferSegment(mem);
            }
            else
            {
                tail = tail!.Append(mem);
            }
        }
    }
}