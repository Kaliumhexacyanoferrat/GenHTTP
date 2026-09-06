using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Ioxide.Protocol.Requests;

/// <summary>Name/value pairs kept as bytes, serving as both headers and query.</summary>
internal sealed class StreamedKeyValueList : IRequestHeaders, IRequestQuery
{
    private readonly List<(ReadOnlyMemory<byte> Name, ReadOnlyMemory<byte> Value)> _entries;

    private (ReadOnlyMemory<byte> Name, ReadOnlyMemory<byte> Value)? _prepended;

    // Serves one list of name/value pairs as both headers and query parameters. The list is owned
    // by the caller and refilled per stream, so this view is built once and reused.
    internal StreamedKeyValueList(List<(ReadOnlyMemory<byte> Name, ReadOnlyMemory<byte> Value)> entries)
    {
        _entries = entries;
    }

    // An entry presented first without copying the list to make room for it - the header path uses
    // it to fold :authority in as a Host header. Null clears it for the next stream.
    internal void Prepend((ReadOnlyMemory<byte> Name, ReadOnlyMemory<byte> Value)? prepended) => _prepended = prepended;

    public int Count => _entries.Count + (_prepended.HasValue ? 1 : 0);

    // One entry, still as bytes, so nothing is decoded that no handler asks for.
    public KeyValuePair<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>> GetMemoryEntry(int index)
    {
        if (_prepended is { } head)
        {
            if (index == 0)
            {
                return new KeyValuePair<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>>(head.Name, head.Value);
            }

            index--;
        }

        (ReadOnlyMemory<byte> name, ReadOnlyMemory<byte> value) = _entries[index];

        return new KeyValuePair<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>>(name, value);
    }
}
