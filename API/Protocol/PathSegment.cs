using System.Buffers;
using System.Diagnostics;

namespace GenHTTP.Api.Protocol;

/// <summary>
/// A single percent-encoded path segment.
/// </summary>
[MemoryView]
public readonly partial struct PathSegment
{

    #region Functionality

    /// <summary>Percent-decodes this segment and returns the human-readable string.</summary>
    public string Decode()
    {
        var span = Value.Span;

        if (span.IndexOf((byte)'%') < 0)
        {
            return System.Text.Encoding.ASCII.GetString(span);
        }

        byte[]? rented = null;

        var buffer = span.Length <= 256
            ? stackalloc byte[span.Length]
            : (rented = ArrayPool<byte>.Shared.Rent(span.Length));

        try
        {
            int write = 0, i = 0;

            while (i < span.Length)
            {
                if (span[i] == '%' && i + 2 < span.Length && IsHexDigit(span[i + 1]) && IsHexDigit(span[i + 2]))
                {
                    buffer[write++] = (byte)((HexValue(span[i + 1]) << 4) | HexValue(span[i + 2]));
                    i += 3;
                }
                else
                {
                    buffer[write++] = span[i++];
                }
            }

            return System.Text.Encoding.UTF8.GetString(buffer[..write]);
        }
        finally
        {
            if (rented != null)
            {
                ArrayPool<byte>.Shared.Return(rented);
            }
        }
    }

    private static bool IsHexDigit(byte b) => (uint)(b - '0') <= 9 || (uint)((b | 0x20) - 'a') <= 5;

    private static int HexValue(byte b) => b <= '9' ? b - '0' : (b | 0x20) - 'a' + 10;

    #endregion

}
