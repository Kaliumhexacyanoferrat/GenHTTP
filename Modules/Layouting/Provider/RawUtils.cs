using System.Runtime.CompilerServices;
using System.Text;

namespace GenHTTP.Modules.Layouting.Provider;

// todo: where to put this?

public static class RawUtils
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Hash(this ReadOnlyMemory<byte> data)
    {
        const uint fnvOffset = 2166136261;
        const uint fnvPrime  = 16777619;

        var hash = fnvOffset;

        foreach (var b in data.Span)
        {
            hash ^= b;
            hash *= fnvPrime;
        }

        return unchecked((int)hash);
    }

    public static int Hash(this string data) => Hash(Encoding.ASCII.GetBytes(data));

}
