using System.Buffers;
using System.Diagnostics;

using GenHTTP.Api;

namespace GenHTTP.Api.Protocol;

/// <summary>
/// A single percent-encoded path segment backed by a raw ASCII byte sequence.
/// Equality is case-insensitive for ASCII letters, matching URI normalisation rules.
/// </summary>
/// <remarks>
/// <c>GenerateToString = false</c> because <c>ToString()</c> should return the decoded
/// (human-readable) form via <see cref="Decode"/>, not the raw encoded bytes.
/// </remarks>
[MemoryView(GenerateToString = false)]
[DebuggerDisplay("{DebuggerValue,nq}")]
public readonly partial struct PathSegment
{

    #region Functionality

    private string DebuggerValue => Decode();

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
