using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Ioxide.Protocol.Requests;

/// <summary>Name/value pairs kept as bytes, serving as both headers and query.</summary>
internal sealed class StreamedKeyValueList : IRequestHeaders, IRequestQuery
{
    private readonly List<(ReadOnlyMemory<byte> Name, ReadOnlyMemory<byte> Value)> _entries;

    // Serves one list of name/value pairs as both headers and query parameters.
    internal StreamedKeyValueList(List<(ReadOnlyMemory<byte> Name, ReadOnlyMemory<byte> Value)> entries)
    {
        _entries = entries;
    }

    public int Count => _entries.Count;

    // One entry, still as bytes, so nothing is decoded that no handler asks for.
    public KeyValuePair<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>> GetMemoryEntry(int index)
    {
        (ReadOnlyMemory<byte> name, ReadOnlyMemory<byte> value) = _entries[index];

        return new KeyValuePair<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>>(name, value);
    }
}
