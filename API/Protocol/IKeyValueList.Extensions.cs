using System.Text;

namespace GenHTTP.Api.Protocol;

public static class IKeyValueListExtensions
{
    
    public static ReadOnlyMemory<byte>? GetEntry(this IKeyValueList list, ReadOnlyMemory<byte> key)
    {
        for (var i = 0; i < list.Count; i++)
        {
            var entry = list[i];

            if (entry.Key.Span.SequenceEqual(key.Span))
            {
                return entry.Value;
            }
        }

        return null;
    }

    public static string? GetEntry(this IKeyValueList list, string key)
    {
        var entry = list.GetEntry(Encoding.ASCII.GetBytes(key));

        if (entry != null)
        {
            return Encoding.ASCII.GetString(entry.Value.Span);
        }

        return null;
    }

    public static bool ContainsKey(this IKeyValueList list, ReadOnlyMemory<byte> key)
    {
        var keySpan = key.Span;

        for (var i = 0; i < list.Count; i++)
        {
            if (list[i].Key.Span.SequenceEqual(keySpan))
                return true;
        }

        return false;
    }

    public static bool ContainsKey(this IKeyValueList list, string key)
        => list.ContainsKey(Encoding.ASCII.GetBytes(key));
    
}
