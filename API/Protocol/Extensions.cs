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


}
