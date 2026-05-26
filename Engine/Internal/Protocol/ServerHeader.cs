using System.Runtime.CompilerServices;
using System.Text;
using GenHTTP.Engine.Internal.Context;

namespace GenHTTP.Engine.Internal.Protocol;

internal static class ServerHeader
{
    private static ReadOnlyMemory<byte> _cached;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ReadOnlyMemory<byte> GetValue(ClientContext context)
    {
        if (!_cached.IsEmpty)
        {
            return _cached;
        }

        var version = context.Server.Version;
        var buffer = new byte[16 + version.Length + 2];

        "Server: GenHTTP/"u8.CopyTo(buffer);
        Encoding.ASCII.GetBytes(version, buffer.AsSpan(16));
        "\r\n"u8.CopyTo(buffer.AsSpan(16 + version.Length));

        _cached = buffer;
        return _cached;
    }
    
}
