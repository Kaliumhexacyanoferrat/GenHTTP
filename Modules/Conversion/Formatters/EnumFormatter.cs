using System.Text;

using GenHTTP.Api.Protocol;

using StringContent = GenHTTP.Modules.IO.Strings.StringContent;

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

    public T Read<T>(ByteString value)
        => throw new NotSupportedException("Enums are explicitly supported by code generation");

    public string? Write(object value, Type type) => value.ToString();

    public IResponseContent GetContent<T>(T value) => new StringContent(value!.ToString()!);

}
