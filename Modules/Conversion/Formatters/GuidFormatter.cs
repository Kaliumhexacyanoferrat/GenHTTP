namespace GenHTTP.Modules.Conversion.Formatters;

public sealed class GuidFormatter : IFormatter
{

    public bool CanHandle(Type type) => type == typeof(Guid);

    public object Read(ReadOnlyMemory<byte> value, Type type)
    {
        var span = value.Span;

        if (Guid.TryParse(span, out var guid))
        {
            return guid;
        }

        throw new ArgumentException("Input does not match required format (GUID)");
    }

    public string? Write(object value, Type type) => value.ToString();
    
}
