namespace GenHTTP.Modules.Conversion.Formatters;

public sealed class GuidFormatter : IFormatter
{

    public bool CanHandle(Type type) => type == typeof(Guid);

    public object? Read(string value, Type type) => Guid.Parse(value);

    public string? Write(object value, Type type) => value.ToString();

}
