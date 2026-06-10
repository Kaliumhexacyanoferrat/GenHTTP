using System.Globalization;
using System.Text;

using GenHTTP.Api.Protocol;

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
        => throw new NotSupportedException("Formattables are explicitly supported by code generation");

    public string? Write(object value, Type type) => Convert.ToString(value, CultureInfo.InvariantCulture);

    public IResponseContent GetContent<T>(T value)
        => throw new NotSupportedException("Formattables are explicitly supported by code generation");

    private static void Throw(Type type, ReadOnlySpan<byte> span) => throw new ArgumentException($"Invalid value '{Encoding.ASCII.GetString(span)}' for primitive type {type.Name}");

}
