using System.Text;

namespace GenHTTP.Modules.Conversion.Formatters;

public sealed class StringFormatter : IFormatter
{

    public bool CanHandle(Type type) => type == typeof(string);

    public object Read(ReadOnlyMemory<byte> value, Type type) => Encoding.UTF8.GetString(value.Span);

    public string Write(object value, Type type) => (string)value;
    
}
