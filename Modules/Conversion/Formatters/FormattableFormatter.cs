using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO.Formattable;

namespace GenHTTP.Modules.Conversion.Formatters;

public sealed class FormattableFormatter : IFormatter
{

    public bool CanHandle(Type type)
        => type.IsAssignableTo(typeof(IUtf8SpanFormattable));

    public object Read(ByteString value, Type type)
    {
        var span = value.Bytes.Span;

        if (type == typeof(int))
            return int.Parse(span, CultureInfo.InvariantCulture);

        if (type == typeof(long))
            return long.Parse(span, CultureInfo.InvariantCulture);

        if (type == typeof(short))
            return short.Parse(span, CultureInfo.InvariantCulture);

        if (type == typeof(byte))
            return byte.Parse(span, CultureInfo.InvariantCulture);

        if (type == typeof(uint))
            return uint.Parse(span, CultureInfo.InvariantCulture);

        if (type == typeof(ulong))
            return ulong.Parse(span, CultureInfo.InvariantCulture);

        if (type == typeof(float))
            return float.Parse(span, CultureInfo.InvariantCulture);

        if (type == typeof(double))
            return double.Parse(span, CultureInfo.InvariantCulture);

        if (type == typeof(decimal))
            return decimal.Parse(span, CultureInfo.InvariantCulture);

        if (type == typeof(Guid))
            return Guid.Parse(span);

        Throw(type, span);
        return default!;
    }

    public T Read<T>(ByteString value)
    {
        var span = value.Bytes.Span;

        if (typeof(T) == typeof(int))
        {
            var v = int.Parse(span, CultureInfo.InvariantCulture);
            return Unsafe.As<int, T>(ref v);
        }

        if (typeof(T) == typeof(long))
        {
            var v = long.Parse(span, CultureInfo.InvariantCulture);
            return Unsafe.As<long, T>(ref v);
        }

        if (typeof(T) == typeof(short))
        {
            var v = short.Parse(span, CultureInfo.InvariantCulture);
            return Unsafe.As<short, T>(ref v);
        }

        if (typeof(T) == typeof(byte))
        {
            var v = byte.Parse(span, CultureInfo.InvariantCulture);
            return Unsafe.As<byte, T>(ref v);
        }

        if (typeof(T) == typeof(uint))
        {
            var v = uint.Parse(span, CultureInfo.InvariantCulture);
            return Unsafe.As<uint, T>(ref v);
        }

        if (typeof(T) == typeof(ulong))
        {
            var v = ulong.Parse(span, CultureInfo.InvariantCulture);
            return Unsafe.As<ulong, T>(ref v);
        }

        if (typeof(T) == typeof(float))
        {
            var v = float.Parse(span, CultureInfo.InvariantCulture);
            return Unsafe.As<float, T>(ref v);
        }

        if (typeof(T) == typeof(double))
        {
            var v = double.Parse(span, CultureInfo.InvariantCulture);
            return Unsafe.As<double, T>(ref v);
        }

        if (typeof(T) == typeof(decimal))
        {
            var v = decimal.Parse(span, CultureInfo.InvariantCulture);
            return Unsafe.As<decimal, T>(ref v);
        }

        if (typeof(T) == typeof(Guid))
        {
            var v = Guid.Parse(span);
            return Unsafe.As<Guid, T>(ref v);
        }

        Throw(typeof(T), span);
        return default!;
    }

    public string? Write(object value, Type type) => Convert.ToString(value, CultureInfo.InvariantCulture);

    public IResponseContent GetContent<T>(T value)
    {
        if (value is IUtf8SpanFormattable formattable)
        {
            return new FormattableContent(formattable);
        }

        throw new ArgumentException();
    }

    private static void Throw(Type type, ReadOnlySpan<byte> span) => throw new ArgumentException($"Invalid value '{Encoding.ASCII.GetString(span)}' for primitive type {type.Name}");

}
