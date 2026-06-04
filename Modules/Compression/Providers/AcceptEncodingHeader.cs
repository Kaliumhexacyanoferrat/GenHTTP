using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Compression.Providers;

public static class AcceptEncodingHeader
{

    public static HashSet<AlgorithmName> ParseSupported(ByteString acceptHeader)
    {
        var result = new HashSet<AlgorithmName>();
        var start = 0;

        var span = acceptHeader.Bytes.Span;

        while (start < span.Length)
        {
            var comma = span[start..].IndexOf((byte)',');
            var end = comma >= 0 ? start + comma : span.Length;

            var token = span.Slice(start, end - start);

            var semicolon = token.IndexOf((byte)';');
            var nameSpan = semicolon >= 0 ? token[..semicolon] : token;

            var part = TrimAscii(nameSpan);

            if (!part.IsEmpty)
            {
                result.Add(new(part.ToArray()));
            }

            start = end + 1;
        }

        return result;
    }

    private static ReadOnlySpan<byte> TrimAscii(ReadOnlySpan<byte> span)
    {
        var start = 0;
        var end = span.Length - 1;

        while (start <= end && IsAsciiWhiteSpace(span[start]))
            start++;

        while (end >= start && IsAsciiWhiteSpace(span[end]))
            end--;

        return span.Slice(start, end - start + 1);
    }

    private static bool IsAsciiWhiteSpace(byte b)
    {
        return b == (byte)' ' || b == (byte)'\t';
    }

}
