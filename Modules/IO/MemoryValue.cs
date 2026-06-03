using System.Runtime.CompilerServices;

namespace GenHTTP.Modules.IO;

public static class MemoryValue
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Equal(ReadOnlyMemory<byte> first, ReadOnlyMemory<byte> second)
    {
        var a = first.Span;
        var b = second.Span;

        if (a.Length != b.Length)
        {
            return false;
        }

        if (a.SequenceEqual(b))
        {
            return true;
        }

        for (var i = 0; i < a.Length; i++)
        {
            var ca = a[i];
            var cb = b[i];

            var la = Normalize(ca);
            var lb = Normalize(cb);

            if (la != lb)
            {
                return false;
            }
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte Normalize(byte value)
    {
        var lower = (byte)(value | 0x20);

        return (uint)(lower - 'a') <= ('z' - 'a') ? lower : value;
    }

}
