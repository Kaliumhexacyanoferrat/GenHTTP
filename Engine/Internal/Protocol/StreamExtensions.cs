using System.Runtime.CompilerServices;

namespace GenHTTP.Engine.Internal.Protocol;

internal static class StreamExtensions
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Write(this Stream stream, string value)
    {
        Span<byte> buffer = stackalloc byte[value.Length];

        for (var i = 0; i < value.Length; i++)
        {
            buffer[i] = (byte)value[i];
        }

        stream.Write(buffer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Write(this Stream stream, long number)
    {
        Span<byte> buffer = stackalloc byte[20];

        if (number.TryFormat(buffer, out var written))
        {
            stream.Write(buffer[..written]);
        }
        else
        {
            throw new InvalidOperationException("Unable to write number to stream");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Write(this Stream stream, ulong number)
    {
        Span<byte> buffer = stackalloc byte[20];

        if (number.TryFormat(buffer, out var written))
        {
            stream.Write(buffer[..written]);
        }
        else
        {
            throw new InvalidOperationException("Unable to write number to stream");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Write(this Stream stream, DateTime time)
    {
        Span<char> charBuffer = stackalloc char[29]; // RFC1123 format is 29 chars

        if (time.ToUniversalTime().TryFormat(charBuffer, out var written, "r"))
        {
            Span<byte> byteBuffer = stackalloc byte[written];

            for (var i = 0; i < written; i++)
            {
                byteBuffer[i] = (byte)charBuffer[i];
            }

            stream.Write(byteBuffer);
        }
        else
        {
            throw new InvalidOperationException("Unable to write date time to stream");
        }
    }

}
