using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Shared.Types;

public class EditableKeyValueList : IKeyValueList
{
    private readonly List<KeyValuePair<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>>> _store = [];

    public int Count => _store.Count;

    public KeyValuePair<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>> this[int index] => _store[index];

    public void Add(ByteString key, ByteString value)
    {
        _store.Add(new KeyValuePair<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>>(key.Bytes, value.Bytes));
    }

    public void Clear() => _store.Clear();

}
