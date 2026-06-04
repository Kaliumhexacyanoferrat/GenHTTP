namespace GenHTTP.Api.Protocol;

public static class IKeyValueListExtensions
{
    
    public static ByteString? GetEntry(this IKeyValueList list, ByteString key)
    {
        for (var i = 0; i < list.Count; i++)
        {
            var entry = list[i];

            if (entry.Key == key)
            {
                return entry.Value;
            }
        }

        return null;
    }

    public static string? GetEntry(this IKeyValueList list, string key)
        => list.GetEntry(new ByteString(key))?.ToString();

    public static bool ContainsKey(this IKeyValueList list, ByteString key)
    {
        for (var i = 0; i < list.Count; i++)
        {
            if (list[i].Key == key)
                return true;
        }

        return false;
    }

    public static bool ContainsKey(this IKeyValueList list, string key)
        => list.ContainsKey(new ByteString(key));
    
}
