using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Conversion.Formatters;

public sealed class BoolFormatter : IFormatter
{
    private static readonly ByteString SwitchOne = new("1");
    private static readonly ByteString SwitchZero = new("0");

    private static readonly ByteString SwitchOn = new("on");
    private static readonly ByteString SwitchOff = new("off");

    private static readonly ByteString SwitchTrue = new("true");
    private static readonly ByteString SwitchFalse = new("false");

    public bool CanHandle(Type type) => type == typeof(bool);

    public object? Read(ByteString value, Type type)
    {
        if (value == SwitchOne || value == SwitchOn || value == SwitchTrue)
        {
            return true;
        }
        if (value == SwitchZero || value == SwitchOff || value == SwitchFalse)
        {
            return false;
        }

        return null;
    }

    public string Write(object value, Type type) => (bool)value ? "1" : "0";

}
