using System.Runtime.CompilerServices;

namespace GenHTTP.Api.Protocol;

/// <summary>
/// Shared low level helpers to decode percent-encoded ("URL encoded") bytes.
/// </summary>
public static class PercentEncoding
{

    /// <summary>
    /// Determines whether the given byte is a valid hexadecimal digit (0-9, a-f, A-F).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsHexDigit(byte value) => (uint)(value - '0') <= 9 || (uint)((value | 0x20) - 'a') <= 5;

    /// <summary>
    /// Converts a hexadecimal digit byte into its numeric value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int HexValue(byte value) => value <= '9' ? value - '0' : (value | 0x20) - 'a' + 10;

    /// <summary>
    /// Attempts to decode the two hex digits following a "%" escape sequence.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryDecode(byte high, byte low, out byte value)
    {
        if (!IsHexDigit(high) || !IsHexDigit(low))
        {
            value = 0;
            return false;
        }

        value = (byte)((HexValue(high) << 4) | HexValue(low));
        return true;
    }

    /// <summary>
    /// Decodes a percent-encoded ("URL encoded") sequence of bytes into the given buffer.
    /// </summary>
    /// <param name="source">The encoded bytes to decode</param>
    /// <param name="target">The buffer the decoded bytes are written to (must be at least as large as <paramref name="source"/>)</param>
    /// <param name="decodePlus">Whether a "+" character should be decoded into a space (as used by form encoded content)</param>
    /// <returns>The number of bytes written to <paramref name="target"/></returns>
    public static int Decode(ReadOnlySpan<byte> source, Span<byte> target, bool decodePlus = false)
    {
        var write = 0;

        for (var read = 0; read < source.Length; read++)
        {
            var current = source[read];

            if (decodePlus && current == (byte)'+')
            {
                target[write++] = (byte)' ';
            }
            else if (current == (byte)'%' && read + 2 < source.Length && TryDecode(source[read + 1], source[read + 2], out var decoded))
            {
                target[write++] = decoded;
                read += 2;
            }
            else
            {
                target[write++] = current;
            }
        }

        return write;
    }

}
