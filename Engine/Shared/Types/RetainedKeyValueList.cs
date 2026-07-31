using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Shared.Types;

public sealed class RetainedKeyValueList : IRequestHeaders, IRequestQuery
{
    private readonly List<KeyValuePair<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>>> _items;

    public int Count => _items.Count;

    public RetainedKeyValueList(IKeyValueList source)
    {
        _items = new(source.Count);

        for (var i = 0; i < source.Count; i++)
        {
            var pair = source.GetMemoryEntry(i);

            _items.Add(new (pair.Key.ToArray(), pair.Value.ToArray()));
        }
    }

    public KeyValuePair<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>> GetMemoryEntry(int index) => _items[index];

}
