using System.Buffers;
using System.Text;

namespace GenHTTP.Engine.Protocol.Parser.Conversion
{

    internal static class ValueConverter
    {

        private static readonly Encoding ASCII = Encoding.ASCII;

        internal static bool CompareTo(ReadOnlySequence<byte> buffer, string expected)
        {
            var i = 0;

            if (buffer.Length != expected.Length)
            {
                return false;
            }

            foreach (var segment in buffer)
            {
                for (int j = 0; j < segment.Length; j++)
                {
                    if (segment.Span[j] != expected[i++])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        internal static string GetString(ReadOnlySequence<byte> buffer)
        {
            return string.Create((int)buffer.Length, buffer, (span, sequence) =>
            {
                foreach (var segment in sequence)
                {
                    ASCII.GetChars(segment.Span, span);
                    span = span[segment.Length..];
                }
            });
        }

    }

}
