using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Ioxide.Protocol.Multiplexed;

/// <summary>
/// Header and query lists over fields a multiplexed protocol has already decoded.
/// </summary>
/// <remarks>
/// The shared list wraps Glyph11's parse output and so assumes HTTP/1.1. HPACK and QPACK hand over
/// name/value pairs instead, with no request line and no raw header block to point back at.
/// </remarks>
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
