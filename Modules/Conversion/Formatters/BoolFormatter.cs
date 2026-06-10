using System.Runtime.CompilerServices;

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

    private static readonly StringContent TrueContent = new("1");
    private static readonly StringContent FalseContent = new("0");

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

        throw new ArgumentException("Cannot convert given value to bool");
    }

    public T Read<T>(ByteString value)
    {
        if (value == SwitchOne || value == SwitchOn || value == SwitchTrue)
        {
            var result = true;
            return Unsafe.As<bool, T>(ref result);
        }
        if (value == SwitchZero || value == SwitchOff || value == SwitchFalse)
        {
            var result = false;
            return Unsafe.As<bool, T>(ref result);
        }

        throw new ArgumentException("Cannot convert given value to bool");
    }

    public string Write(object value, Type type) => (bool)value ? "1" : "0";

    public IResponseContent GetContent<T>(T value)
    {
        if (value is bool b)
            return b ? TrueContent : FalseContent;

        throw new NotSupportedException();
    }

}
