using System.Text;
using GenHTTP.Api.Protocol;
using StringContent = GenHTTP.Modules.IO.Strings.StringContent;

namespace GenHTTP.Modules.Conversion.Formatters;

public sealed class StringFormatter : IFormatter
{

    public bool CanHandle(Type type) => type == typeof(string);

    public object Read(ByteString value, Type type) => Encoding.UTF8.GetString(value.Bytes.Span);

    public T Read<T>(ByteString value)
        => throw new NotSupportedException("Strings are explicitly supported by code generation");

    public string Write(object value, Type type) => (string)value;

    public IResponseContent GetContent<T>(T value)
        => throw new NotSupportedException("Strings are explicitly supported by code generation");

}
