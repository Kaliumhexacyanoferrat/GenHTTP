﻿using System.Buffers;
using System.Text;

namespace GenHTTP.Engine.Internal.Protocol.Parser.Conversion;

internal static class ValueConverter
{
    private static readonly Encoding Ascii = Encoding.ASCII;

    internal static bool CompareTo(ReadOnlySequence<byte> buffer, string expected)
    {
        var i = 0;

        if (buffer.Length != expected.Length)
        {
            return false;
        }

        foreach (var segment in buffer)
        {
            for (var j = 0; j < segment.Length; j++)
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
        if (buffer.Length > 0)
        {
            var result = string.Create((int)buffer.Length, buffer, (span, sequence) =>
            {
                foreach (var segment in sequence)
                {
                    Ascii.GetChars(segment.Span, span);
                    span = span[segment.Length..];
                }
            });

            return result.Trim();
        }

        return string.Empty;
    }
}
