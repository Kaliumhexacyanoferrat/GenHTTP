using System.Text;

namespace GenHTTP.Engine.Shared.Types;

public static class Extensions
{
    private static readonly Encoding AsciiEncoding = Encoding.ASCII;

    public static string GetString(this ReadOnlyMemory<byte> memory) => AsciiEncoding.GetString(memory.Span);

    public static ReadOnlyMemory<byte> GetMemory(this string str) => AsciiEncoding.GetBytes(str);

}
