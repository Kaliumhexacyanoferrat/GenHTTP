using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Shared.Types;

public class RetainedKeyValueList : IRequestHeaders, IRequestQuery
{
    private readonly List<KeyValuePair<ByteString, ByteString>> _items;

    public int Count => _items.Count;

    public RetainedKeyValueList(IKeyValueList source)
    {
        _items = new(source.Count);

        for (var i = 0; i < source.Count; i++)
        {
            var pair = source[i];
            
            _items.Add(new (new ByteString(pair.Key.Bytes.ToArray()), new ByteString(pair.Value.Bytes.ToArray())));
        }
    }

    public KeyValuePair<ByteString, ByteString> this[int index] => _items[index];

}
