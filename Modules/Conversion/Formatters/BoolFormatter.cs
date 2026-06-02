using GenHTTP.Modules.IO;

namespace GenHTTP.Modules.Conversion.Formatters;

public sealed class BoolFormatter : IFormatter
{
    private static readonly ReadOnlyMemory<byte> SwitchOne = "1"u8.ToArray();
    private static readonly ReadOnlyMemory<byte> SwitchZero = "0"u8.ToArray();

    private static readonly ReadOnlyMemory<byte> SwitchOn = "on"u8.ToArray();
    private static readonly ReadOnlyMemory<byte> SwitchOff = "off"u8.ToArray();

    private static readonly ReadOnlyMemory<byte> SwitchTrue = "true"u8.ToArray();
    private static readonly ReadOnlyMemory<byte> SwitchFalse = "false"u8.ToArray();

    public bool CanHandle(Type type) => type == typeof(bool);

    public object? Read(ReadOnlyMemory<byte> value, Type type)
    {
        if (MemoryValue.Equal(value, SwitchOne) || MemoryValue.Equal(value, SwitchOn) || MemoryValue.Equal(value, SwitchTrue))
        {
            return true;
        }
        if (MemoryValue.Equal(value, SwitchZero) || MemoryValue.Equal(value, SwitchOff) || MemoryValue.Equal(value, SwitchFalse))
        {
            return false;
        }

        return null;
    }

    public string Write(object value, Type type) => (bool)value ? "1" : "0";

}
