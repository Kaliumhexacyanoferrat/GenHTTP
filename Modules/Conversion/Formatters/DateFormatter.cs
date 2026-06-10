using System.Buffers.Text;
using System.Globalization;
using System.Text;
using StringContent = GenHTTP.Modules.IO.Strings.StringContent;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Conversion.Formatters;

public sealed class DateFormatter : IFormatter
{

    public bool CanHandle(Type type)
    {
        return type == typeof(DateTime)
               || type == typeof(DateTimeOffset)
               || type == typeof(DateOnly)
               || type == typeof(TimeOnly)
               || type == typeof(TimeSpan);
    }

    public object? Read(ByteString value, Type type)
    {
        var span = value.Bytes.Span;

        if (type == typeof(DateTime))
        {
            if (Utf8Parser.TryParse(span, out DateTime result, out var consumed) && consumed == span.Length)
            {
                return result;
            }
        }
        else if (type == typeof(DateTimeOffset))
        {
            if (Utf8Parser.TryParse(span, out DateTimeOffset result, out var consumed) && consumed == span.Length)
            {
                return result;
            }
        }
        else if (type == typeof(DateOnly))
        {
            if (DateOnly.TryParseExact(StringFromUtf8(span), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
            {
                return result;
            }
        }
        else if (type == typeof(TimeOnly))
        {
            if (TimeOnly.TryParse(StringFromUtf8(span), CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
            {
                return result;
            }
        }
        else if (type == typeof(TimeSpan))
        {
            if (Utf8Parser.TryParse(span, out TimeSpan result, out var consumed) && consumed == span.Length)
            {
                return result;
            }
        }

        throw new ArgumentException($"Unable to parse '{value}' as {type.Name}");
    }

    public T Read<T>(ByteString value) => (T)Read(value, typeof(T))!;

    public string? Write(object value, Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;

        Span<byte> buffer = stackalloc byte[64];

        if (type == typeof(DateTime))
        {
            if (Utf8Formatter.TryFormat((DateTime)value, buffer, out var written, 'O'))
            {
                return StringFromUtf8(buffer[..written]);
            }
        }
        else if (type == typeof(DateTimeOffset))
        {
            if (Utf8Formatter.TryFormat((DateTimeOffset)value, buffer, out var written, 'O'))
            {
                return StringFromUtf8(buffer[..written]);
            }
        }
        else if (type == typeof(TimeSpan))
        {
            if (Utf8Formatter.TryFormat((TimeSpan)value, buffer, out var written, 'c'))
            {
                return StringFromUtf8(buffer[..written]);
            }
        }
        else if (type == typeof(DateOnly))
        {
            Span<char> chars = stackalloc char[10];

            if (((DateOnly)value).TryFormat(chars, out var written, "yyyy-MM-dd", CultureInfo.InvariantCulture))
            {
                return new string(chars[..written]);
            }
        }
        else if (type == typeof(TimeOnly))
        {
            Span<char> chars = stackalloc char[16];

            if (((TimeOnly)value).TryFormat(chars, out var written, "HH:mm:ss.fffffff", CultureInfo.InvariantCulture))
            {
                return new string(chars[..written]);
            }
        }

        throw new InvalidOperationException($"Unsupported type {type}");
    }

    public IResponseContent GetContent<T>(T value)
    {
        var text = Write(value!, typeof(T)) ?? string.Empty;

        return new StringContent(text);
    }

    private static string StringFromUtf8(ReadOnlySpan<byte> utf8)
        => Encoding.UTF8.GetString(utf8);

}
