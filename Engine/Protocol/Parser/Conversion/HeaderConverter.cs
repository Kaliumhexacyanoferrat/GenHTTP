using System.Buffers;

namespace GenHTTP.Engine.Protocol.Parser.Conversion
{

    internal static class HeaderConverter
    {
        private static readonly string[] KNOWN_HEADERS = new[] { "Host", "User-Agent", "Accept", "Content-Type", "Content-Length" };

        private static readonly string[] KNOWN_VALUES = new[] { "*/*" };

        internal static string ToKey(ReadOnlySequence<byte> value)
        {
            foreach (var known in KNOWN_HEADERS)
            {
                if (ValueConverter.CompareTo(value, known)) return known;
            }

            return ValueConverter.GetString(value);
        }

        internal static string ToValue(ReadOnlySequence<byte> value)
        {
            foreach (var known in KNOWN_VALUES)
            {
                if (ValueConverter.CompareTo(value, known)) return known;
            }

            return ValueConverter.GetString(value);
        }

    }

}
