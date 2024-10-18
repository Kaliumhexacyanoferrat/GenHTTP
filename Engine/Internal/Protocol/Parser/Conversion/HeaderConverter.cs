using System.Buffers;

namespace GenHTTP.Engine.Internal.Protocol.Parser.Conversion;

internal static class HeaderConverter
{
    private static readonly string[] KnownHeaders = ["Host", "User-Agent", "Accept", "Content-Type", "Content-Length"];

    private static readonly string[] KnownValues = ["*/*"];

    internal static string ToKey(ReadOnlySequence<byte> value)
    {
        foreach (var known in KnownHeaders)
        {
            if (ValueConverter.CompareTo(value, known))
            {
                return known;
            }
        }

        return ValueConverter.GetString(value);
    }

    internal static string ToValue(ReadOnlySequence<byte> value)
    {
        foreach (var known in KnownValues)
        {
            if (ValueConverter.CompareTo(value, known))
            {
                return known;
            }
        }

        return ValueConverter.GetString(value);
    }
}
