using System.Buffers;
using System.Runtime.CompilerServices;

namespace GenHTTP.Engine.Internal.Protocol;

internal static class BufferWriterExtensions
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Write(this IBufferWriter<byte> writer, ulong number)
    {
        Span<byte> buffer = writer.GetSpan(20);

        if (number.TryFormat(buffer, out var written))
        {
            writer.Advance(written);
        }
        else
        {
            throw new InvalidOperationException("Unable to write number to buffer");
        }
    }

}
