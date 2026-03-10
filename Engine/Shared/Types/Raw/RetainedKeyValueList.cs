using GenHTTP.Api.Protocol.Raw;

namespace GenHTTP.Engine.Shared.Types;

public class RetainedKeyValueList : IRawKeyValueList
{
    private readonly List<KeyValuePair<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>>> _items;

    public int Count => _items.Count;

    public RetainedKeyValueList(IRawKeyValueList source)
    {
        _items = new(source.Count);

        for (int i = 0; i < source.Count; i++)
        {
            var pair = source[i];
            
            _items.Add(new (pair.Key.ToArray(), pair.Value.ToArray()));
        }
    }

    public KeyValuePair<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>> this[int index] => _items[index];

}
