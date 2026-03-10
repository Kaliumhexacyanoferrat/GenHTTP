namespace GenHTTP.Api.Protocol.Raw;

public interface IRawKeyValueList
{

    int Count { get; }

    KeyValuePair<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>> this[int index] { get; }

    bool ContainsKey(ReadOnlyMemory<byte> key)
    {
        var keySpan = key.Span;

        for (var i = 0; i < Count; i++)
        {
            if (this[i].Key.Span.SequenceEqual(keySpan))
                return true;
        }

        return false;
    }

}
