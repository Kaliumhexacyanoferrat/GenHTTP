using GenHTTP.Api.Protocol;

using StringContent = GenHTTP.Modules.IO.Strings.StringContent;

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

    public T Read<T>(ByteString value)
    {
        if (value == SwitchOne || value == SwitchOn || value == SwitchTrue)
        {
            return (T)(object)true;
        }
        if (value == SwitchZero || value == SwitchOff || value == SwitchFalse)
        {
            return (T)(object)false;
        }

        throw new ArgumentException();
    }

    public string Write(object value, Type type) => (bool)value ? "1" : "0";

    public IResponseContent GetContent<T>(T value)
    {
        if (value is bool b)
            return new StringContent(b ? "1" : "0");

        throw new NotSupportedException();
    }

}
