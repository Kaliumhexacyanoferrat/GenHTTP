namespace GenHTTP.Api.Protocol.Raw;

public static class Extensions
{

    public static ReadOnlyMemory<byte>? GetEntry(this IRawKeyValueList list, ReadOnlyMemory<byte> key)
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

}
