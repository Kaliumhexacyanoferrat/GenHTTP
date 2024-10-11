namespace GenHTTP.Modules.Conversion.Formatters;

public sealed class BoolFormatter : IFormatter
{

    public bool CanHandle(Type type) => type == typeof(bool);

    public object? Read(string value, Type type)
    {
        if (value == "1" || Compare(value, "on") || Compare(value, "true"))
        {
            return true;
        }
        if (value == "0" || Compare(value, "off") || Compare(value, "false"))
        {
            return false;
        }

        return null;
    }

    public string? Write(object value, Type type) => (bool)value ? "1" : "0";

    private static bool Compare(string value, string expected) => string.Equals(value, expected, StringComparison.InvariantCultureIgnoreCase);
}
