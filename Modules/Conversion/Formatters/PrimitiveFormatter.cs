using System.Buffers.Text;
using System.Globalization;
using System.Text;

namespace GenHTTP.Modules.Conversion.Formatters;

public sealed class PrimitiveFormatter : IFormatter
{
    
    public bool CanHandle(Type type) => type.IsPrimitive;

    public object Read(ReadOnlyMemory<byte> value, Type type)
    {
        var span = value.Span;

        if (type == typeof(int))
        {
            if (Utf8Parser.TryParse(span, out int i, out _, 'D'))
                return i;
        }
        else if (type == typeof(long))
        {
            if (Utf8Parser.TryParse(span, out long l, out _, 'D'))
                return l;
        }
        else if (type == typeof(short))
        {
            if (Utf8Parser.TryParse(span, out short s, out _, 'D'))
                return s;
        }
        else if (type == typeof(byte))
        {
            if (Utf8Parser.TryParse(span, out byte b, out _, 'D'))
                return b;
        }
        else if (type == typeof(bool))
        {
            if (TryParseBool(span, out var b))
                return b;
        }
        else if (type == typeof(double))
        {
            if (Utf8Parser.TryParse(span, out double d, out _, 'G'))
                return d;
        }
        else if (type == typeof(float))
        {
            if (Utf8Parser.TryParse(span, out float f, out _, 'G'))
                return f;
        }
        else if (type == typeof(char))
        {
            if (span.Length == 1)
                return (char)span[0];
        }

        Throw(type, span);
        return default!;
    }

    public string? Write(object value, Type type) => Convert.ToString(value, CultureInfo.InvariantCulture);

    private static bool TryParseBool(ReadOnlySpan<byte> span, out bool value)
    {
        if (span.Length == 4 && (span[0] | 0x20) == 't' && (span[1] | 0x20) == 'r' && (span[2] | 0x20) == 'u' && (span[3] | 0x20) == 'e')
        {
            value = true;
            return true;
        }

        if (span.Length == 5 && (span[0] | 0x20) == 'f' && (span[1] | 0x20) == 'a' && (span[2] | 0x20) == 'l' && (span[3] | 0x20) == 's' && (span[4] | 0x20) == 'e')
        {
            value = false;
            return true;
        }

        value = default;
        return false;
    }

    private static void Throw(Type type, ReadOnlySpan<byte> span) => throw new ArgumentException($"Invalid value '{Encoding.ASCII.GetString(span)}' for primitive type {type.Name}");
    
}