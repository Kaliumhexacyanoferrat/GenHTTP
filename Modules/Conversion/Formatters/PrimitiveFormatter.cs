using System.Globalization;

namespace GenHTTP.Modules.Conversion.Formatters;

public sealed class PrimitiveFormatter : IFormatter
{

    public bool CanHandle(Type type) => type.IsPrimitive;

    public object? Read(string value, Type type) => Convert.ChangeType(value, type, CultureInfo.InvariantCulture);

    public string? Write(object value, Type type) => Convert.ChangeType(value, typeof(string), CultureInfo.InvariantCulture) as string;

}
