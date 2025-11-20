using System.Buffers;

namespace GenHTTP.Engine.Internal.Protocol.Parser.Conversion;

internal static class QueryConverter
{

    internal static void Emit(Request request, ReadOnlySequence<byte> value)
    {
        if (!value.IsEmpty)
        {
            var reader = new SequenceReader<byte>(value);

            while (reader.TryReadTo(out ReadOnlySequence<byte> segment, (byte)'&'))
            {
                EmitSegment(request, segment);
            }

            if (!reader.End)
            {
                var remainder = reader.Sequence.Slice(reader.Position);
                EmitSegment(request, remainder);
            }
        }
    }

    private static void EmitSegment(Request request, ReadOnlySequence<byte> segment)
    {
        if (!segment.IsEmpty)
        {
            var reader = new SequenceReader<byte>(segment);

            string? name, value = null;

            if (reader.TryReadTo(out ReadOnlySequence<byte> firstSegment, (byte)'='))
            {
                name = ValueConverter.GetString(firstSegment);

                if (!reader.End)
                {
                    var remainingValue = reader.Sequence.Slice(reader.Position);
                    value = ValueConverter.GetString(remainingValue);
                }
            }
            else
            {
                var remainingName = reader.Sequence.Slice(reader.Position);
                name = ValueConverter.GetString(remainingName);
            }

            request.SetQuery(Uri.UnescapeDataString(name), value != null ? Uri.UnescapeDataString(value) : string.Empty);
        }
    }
    
}
