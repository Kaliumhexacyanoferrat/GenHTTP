using System.Text;

namespace GenHTTP.Api.Protocol;

// todo: seperate extensions by class

public static class Extensions
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

    public static ReadOnlyMemory<byte> WithoutOptions(this ReadOnlyMemory<byte> value)
    {
        var span = value.Span;
        var idx = span.IndexOf((byte)';');

        if (idx < 0)
        {
            return value;
        }

        var trimmed = span[..idx];

        while (!trimmed.IsEmpty && trimmed[^1] == (byte)' ')
        {
            trimmed = trimmed[..^1];
        }

        return trimmed.ToArray();
    }

}
