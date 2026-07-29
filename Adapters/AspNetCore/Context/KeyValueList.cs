using GenHTTP.Api.Protocol;

namespace GenHTTP.Adapters.AspNetCore.Context;

internal sealed class KeyValueList : IRequestHeaders, IRequestQuery
{
    private readonly List<KeyValuePair<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>>> _store = [];

    public int Count => _store.Count;

    public KeyValuePair<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>> GetMemoryEntry(int index) => _store[index];

    public void Add(ByteString key, ByteString value) => _store.Add(new(key.Bytes, value.Bytes));

    public void Clear() => _store.Clear();

}
