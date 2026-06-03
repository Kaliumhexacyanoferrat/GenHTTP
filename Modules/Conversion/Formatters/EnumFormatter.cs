using System.Text;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Conversion.Formatters;

public sealed class EnumFormatter : IFormatter
{

    public bool CanHandle(Type type) => type.IsEnum;

    public object Read(ByteString value, Type type)
    {
        var span = value.Bytes.Span;

        Span<char> buffer = stackalloc char[span.Length];

        for (var i = 0; i < span.Length; i++)
        {
            buffer[i] = (char)span[i];
        }

        if (Enum.TryParse(type, buffer, ignoreCase: false, out var result))
        {
            return result;
        }

        throw new ArgumentException($"Invalid enum value '{Encoding.ASCII.GetString(span)}' for type {type.Name}");
    }

    public string? Write(object value, Type type) => value.ToString();
    
}
