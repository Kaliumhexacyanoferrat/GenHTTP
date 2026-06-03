using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Shared.Types;

public class EditableKeyValueList : IKeyValueList
{
    private readonly List<KeyValuePair<ByteString, ByteString>> _store = [];

    public int Count => _store.Count;

    public KeyValuePair<ByteString, ByteString> this[int index] => _store[index];

    public void Add(ByteString key, ByteString value)
    {
        _store.Add(new KeyValuePair<ByteString, ByteString>(key, value));
    }

    public void Clear() => _store.Clear();

}
