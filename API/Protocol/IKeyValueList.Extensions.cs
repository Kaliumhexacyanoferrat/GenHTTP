namespace GenHTTP.Api.Protocol;

public static class IKeyValueListExtensions
{

    /// <summary>
    /// Tries to fetch the entry with the given key from the list.
    /// </summary>
    /// <param name="list">The list to fetch the entry from</param>
    /// <param name="key">The key of the item to be fetched</param>
    /// <returns>The value of the first matching entry or null, if the entry could not be found</returns>
    public static ByteString? GetEntry(this IKeyValueList list, ByteString key)
    {
        for (var i = 0; i < list.Count; i++)
        {
            var entry = list.GetMemoryEntry(i);

            if (entry.Key == key)
            {
                return new(entry.Value);
            }
        }

        return null;
    }

    /// <summary>
    /// Tries to fetch the entry with the given key from the list.
    /// </summary>
    /// <param name="list">The list to fetch the entry from</param>
    /// <param name="key">The key of the item to be fetched</param>
    /// <returns>The value of the first matching entry or null, if the entry could not be found</returns>
    public static string? GetEntry(this IKeyValueList list, string key)
        => list.GetEntry(new ByteString(key))?.ToString();

    /// <summary>
    /// Checks, whether the list contains the given key.
    /// </summary>
    /// <param name="list">The list to check the entries</param>
    /// <param name="key">The key to search for</param>
    /// <returns>true, if the list contains an entry with that key, otherwise false</returns>
    public static bool ContainsKey(this IKeyValueList list, ByteString key)
    {
        for (var i = 0; i < list.Count; i++)
        {
            if (list.GetMemoryEntry(i).Key == key)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Checks, whether the list contains the given key.
    /// </summary>
    /// <param name="list">The list to check the entries</param>
    /// <param name="key">The key to search for</param>
    /// <returns>true, if the list contains an entry with that key, otherwise false</returns>
    public static bool ContainsKey(this IKeyValueList list, string key)
        => list.ContainsKey(new ByteString(key));

    /// <summary>
    /// Fetches an entry of the list by index and converts it into byte strings.
    /// </summary>
    /// <param name="list">The list to get the entry from</param>
    /// <param name="index">The zero based index of the item to be fetched</param>
    /// <returns>The entry as byte strings</returns>
    public static KeyValuePair<ByteString, ByteString> GetStringEntry(this IKeyValueList list, int index)
    {
        var entry = list.GetMemoryEntry(index);

        return new(new ByteString(entry.Key), new ByteString(entry.Value));
    }

}
