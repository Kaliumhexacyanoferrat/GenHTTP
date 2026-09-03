using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Ioxide.Protocol.Multiplexed;

internal sealed class MultiplexedKeyValueList : IRequestHeaders, IRequestQuery
{
    private readonly List<(ReadOnlyMemory<byte> Name, ReadOnlyMemory<byte> Value)> _entries;

    internal MultiplexedKeyValueList(List<(ReadOnlyMemory<byte> Name, ReadOnlyMemory<byte> Value)> entries)
    {
        _entries = entries;
    }

    public int Count => _entries.Count;

    public KeyValuePair<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>> GetMemoryEntry(int index)
    {
        (ReadOnlyMemory<byte> name, ReadOnlyMemory<byte> value) = _entries[index];

        return new KeyValuePair<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>>(name, value);
    }
}
