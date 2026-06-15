using System.Runtime.CompilerServices;

namespace GenHTTP.Engine.Ioxide.Protocol;

/// <summary>
/// Per-reactor cached "Date: ...\r\n" header, refreshed at most once a second. Mirrors GenHTTP's
/// Internal-engine DateHeader but is <see cref="ThreadStaticAttribute">[ThreadStatic]</see>: each
/// reactor runs on its own thread, so it owns its buffer — no cross-thread races (a shared static
/// would tear under N reactors) and no per-response formatting or allocation.
/// </summary>
internal static class DateHeader
{
    [ThreadStatic]
    private static byte[]? _buffer; // "Date: " (6) + RFC1123 (29) + CRLF (2)

    [ThreadStatic]
    private static int _second;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<byte> Get()
    {
        var buffer = _buffer;

        if (buffer is null)
        {
            buffer = _buffer = new byte[6 + 29 + 2];
            _second = -1;
        }

        var now = DateTime.UtcNow;

        if (now.Second != _second)
        {
            _second = now.Second;

            "Date: "u8.CopyTo(buffer);
            now.TryFormat(buffer.AsSpan(6), out _, "r");
            "\r\n"u8.CopyTo(buffer.AsSpan(35));
        }

        return buffer;
    }
}
