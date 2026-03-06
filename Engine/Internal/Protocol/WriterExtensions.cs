using System.Buffers;
using System.Runtime.CompilerServices;

namespace GenHTTP.Engine.Internal.Protocol;

internal static class BufferWriterExtensions
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Write(this IBufferWriter<byte> writer, string value)
    {
        Span<byte> span = writer.GetSpan(value.Length);

        for (var i = 0; i < value.Length; i++)
        {
            span[i] = (byte)value[i];
        }

        writer.Advance(value.Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Write(this IBufferWriter<byte> writer, long number)
    {
        Span<byte> buffer = stackalloc byte[20];

        if (number.TryFormat(buffer, out var written))
        {
            var span = writer.GetSpan(written);
            buffer[..written].CopyTo(span);
            writer.Advance(written);
        }
        else
        {
            throw new InvalidOperationException("Unable to write number to buffer");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Write(this IBufferWriter<byte> writer, ulong number)
    {
        Span<byte> buffer = stackalloc byte[20];

        if (number.TryFormat(buffer, out var written))
        {
            var span = writer.GetSpan(written);
            buffer[..written].CopyTo(span);
            writer.Advance(written);
        }
        else
        {
            throw new InvalidOperationException("Unable to write number to buffer");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Write(this IBufferWriter<byte> writer, DateTime time)
    {
        Span<char> charBuffer = stackalloc char[29]; // RFC1123 format is 29 chars

        if (time.ToUniversalTime().TryFormat(charBuffer, out var written, "r"))
        {
            var span = writer.GetSpan(written);

            for (var i = 0; i < written; i++)
            {
                span[i] = (byte)charBuffer[i];
            }

            writer.Advance(written);
        }
        else
        {
            throw new InvalidOperationException("Unable to write date time to buffer");
        }
    }

}
