using System.Buffers;

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

}
