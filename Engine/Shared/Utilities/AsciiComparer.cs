using System.Runtime.CompilerServices;

namespace GenHTTP.Engine.Shared.Utilities;

public static class AsciiComparer
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsIgnoreCase(ReadOnlyMemory<byte> a, ReadOnlyMemory<byte> b)
        => EqualsIgnoreCase(a.Span, b.Span);

    public static bool EqualsIgnoreCase(ReadOnlySpan<byte> a, ReadOnlySpan<byte> b)
    {
        if (a.Length != b.Length)
        {
            return false;
        }

        for (var i = 0; i < a.Length; i++)
        {
            var x = a[i];
            var y = b[i];

            if ((uint)(x - 'A') <= 25) x = (byte)(x + 32);
            if ((uint)(y - 'A') <= 25) y = (byte)(y + 32);

            if (x != y)
            {
                return false;
            }
        }

        return true;
    }

}
