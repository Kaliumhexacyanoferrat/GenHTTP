using System;
using System.Buffers;

namespace GenHTTP.Engine.Protocol.Parser.Conversion;

internal static class QueryConverter
{

    internal static RequestQuery? ToQuery(ReadOnlySequence<byte> value)
    {
            if (!value.IsEmpty)
            {
                var query = new RequestQuery();

                var reader = new SequenceReader<byte>(value);

                while (reader.TryReadTo(out ReadOnlySequence<byte> segment, (byte)'&'))
                {
                    AppendSegment(query, segment);
                }

                if (!reader.End)
                {
                    var remainder = reader.Sequence.Slice(reader.Position);
                    AppendSegment(query, remainder);
                }

                return query;
            }

            return null;
        }

    private static void AppendSegment(RequestQuery query, ReadOnlySequence<byte> segment)
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

                query[Uri.UnescapeDataString(name)] = (value != null) ? Uri.UnescapeDataString(value) : string.Empty;
            }
        }

}
