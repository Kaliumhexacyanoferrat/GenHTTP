namespace GenHTTP.Modules.Conversion.Formatters;

public sealed class EnumFormatter : IFormatter
{

    public bool CanHandle(Type type) => type.IsEnum;

    public object Read(string value, Type type) => Enum.Parse(type, value);

    public string? Write(object value, Type type) => value.ToString();

}
