using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Conversion.Formatters;

public sealed class DateOnlyFormatter : IFormatter
{
    
    public bool CanHandle(Type type) => type == typeof(DateOnly);

    public object Read(ByteString value, Type type)
    {
        if (value.Bytes.Length != 10)
            Throw();

        var span = value.Bytes.Span;

        if (span[4] != (byte)'-' || span[7] != (byte)'-')
            Throw();
            
        if (!TryParse4Digits(span, 0, out var year))
            Throw();

        if (!TryParse2Digits(span, 5, out var month))
            Throw();

        if (!TryParse2Digits(span, 8, out var day))
            Throw();

        try
        {
            return new DateOnly(year, month, day);
        }
        catch (Exception e)
        {
            throw new ArgumentException("Input does not match required format (yyyy-MM-dd)", e);
        }
    }

    public string Write(object value, Type type) => ((DateOnly)value).ToString("yyyy-MM-dd");

    private static bool TryParse4Digits(ReadOnlySpan<byte> span, int start, out int value)
    {
        if ((uint)(span[start] - '0') > 9 || (uint)(span[start + 1] - '0') > 9 || (uint)(span[start + 2] - '0') > 9 || (uint)(span[start + 3] - '0') > 9)
        {
            value = 0;
            return false;
        }

        value = (span[start] - '0') * 1000 + (span[start + 1] - '0') * 100 + (span[start + 2] - '0') * 10 + (span[start + 3] - '0');

        return true;
    }

    private static bool TryParse2Digits(ReadOnlySpan<byte> span, int start, out int value)
    {
        if ((uint)(span[start] - '0') > 9 || (uint)(span[start + 1] - '0') > 9)
        {
            value = 0;
            return false;
        }

        value = (span[start] - '0') * 10 + (span[start + 1] - '0');

        return true;
    }

    private static void Throw() => throw new ArgumentException("Input does not match required format (yyyy-MM-dd)");
    
}