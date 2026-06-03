using System.Text;

namespace GenHTTP.Api.Protocol;

public static class IKeyValueListExtensions
{
    
    public static ByteString? GetEntry(this IKeyValueList list, ByteString key)
    {
        for (var i = 0; i < list.Count; i++)
        {
            var entry = list[i];

            if (entry.Key.Bytes.Span.SequenceEqual(key.Bytes.Span))
            {
                return entry.Value;
            }
        }

        return null;
    }

    public static string? GetEntry(this IKeyValueList list, string key)
    {
        var entry = list.GetEntry(new ByteString(Encoding.ASCII.GetBytes(key)));

        if (entry != null)
        {
            return Encoding.ASCII.GetString(entry.Value.Bytes.Span);
        }

        return null;
    }

    public static bool ContainsKey(this IKeyValueList list, ByteString key)
    {
        var keySpan = key.Bytes.Span;

        for (var i = 0; i < list.Count; i++)
        {
            if (list[i].Key.Bytes.Span.SequenceEqual(keySpan))
                return true;
        }

        return false;
    }

    public static bool ContainsKey(this IKeyValueList list, string key)
        => list.ContainsKey(new ByteString(Encoding.ASCII.GetBytes(key)));
    
}
