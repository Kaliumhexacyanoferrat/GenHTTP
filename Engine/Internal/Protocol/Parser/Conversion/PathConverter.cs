using System.Buffers;
using GenHTTP.Api.Routing;

namespace GenHTTP.Engine.Internal.Protocol.Parser.Conversion;

internal static class PathConverter
{
    private static readonly WebPath Root = new(new List<WebPathPart>(), true);

    internal static WebPath ToPath(ReadOnlySequence<byte> value)
    {
        if (value.Length == 1)
        {
            return Root;
        }

        var reader = new SequenceReader<byte>(value);

        reader.Advance(1);

        var parts = new List<WebPathPart>(4);

        while (reader.TryReadTo(out ReadOnlySequence<byte> segment, (byte)'/'))
        {
            parts.Add(new WebPathPart(ValueConverter.GetString(segment)));
        }

        if (!reader.End)
        {
            var remainder = reader.Sequence.Slice(reader.Position);
            parts.Add(new WebPathPart(ValueConverter.GetString(remainder)));

            return new WebPath(parts, false);
        }

        return new WebPath(parts, true);
    }
}
