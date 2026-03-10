using GenHTTP.Api.Protocol.Raw;

namespace GenHTTP.Engine.Shared.Types;

public class EditableKeyValueList : IRawKeyValueList
{
    private readonly List<KeyValuePair<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>>> _store = new();

    public int Count => _store.Count;

    public KeyValuePair<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>> this[int index] => _store[index];

    public void Add(ReadOnlyMemory<byte> key, ReadOnlyMemory<byte> value)
    {
        _store.Add(new KeyValuePair<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>>(key, value));
    }

    public void Clear() => _store.Clear();

}
