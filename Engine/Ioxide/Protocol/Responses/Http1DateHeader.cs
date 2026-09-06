using System.Runtime.CompilerServices;

namespace GenHTTP.Engine.Ioxide.Protocol.Responses;

/// <summary>The Date header, held per reactor and rebuilt once a second.</summary>
// Not Shared's Http1DateHeader, which formats into one static buffer - fine for a single
// connection loop, a race across reactor threads.
internal static class Http1DateHeader
{
    [ThreadStatic]
    private static byte[]? _buffer; // "Date: " (6) + RFC1123 (29) + CRLF (2)

    [ThreadStatic]
    private static int _second;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // The Date header, formatted at most once a second per reactor.
    public static ReadOnlyMemory<byte> Get()
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
