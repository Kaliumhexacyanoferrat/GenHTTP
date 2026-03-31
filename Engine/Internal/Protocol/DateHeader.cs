using System.Runtime.CompilerServices;

namespace GenHTTP.Engine.Internal.Protocol;

/// <summary>
/// Caches the value of the date header for one second
/// before creating a new value, saving some allocations.
/// </summary>
public static class DateHeader
{
    private static readonly byte[] Buffer = new byte[6 + 29 + 2]; // "Date: " + RFC1123 + "\r\n"
    
    private static readonly ReadOnlyMemory<byte> Value = Buffer;

    private static int _second = 61;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyMemory<byte> GetValue()
    {
        var now = DateTime.UtcNow;
        var second = now.Second;

        if (second == _second) return Value;

        _second = second;
        
        "Date: "u8.CopyTo(Buffer);
        
        now.TryFormat(Buffer.AsSpan(6), out _, "r");
        
        "\r\n"u8.CopyTo(Buffer.AsSpan(35));

        return Value;
    }
}