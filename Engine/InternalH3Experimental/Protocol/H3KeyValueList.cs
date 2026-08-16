using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.InternalH3Experimental.Protocol;

/// <summary>
/// Header and query lists over Glyph3's decoded fields.
/// </summary>
/// <remarks>
/// The engine carries its own rather than reusing the shared one, which wraps Glyph11's list and
/// therefore assumes an HTTP/1.1 parse.
/// </remarks>
internal sealed class H3KeyValueList : IRequestHeaders, IRequestQuery
{
    private readonly List<(ReadOnlyMemory<byte> Name, ReadOnlyMemory<byte> Value)> _entries;

    internal H3KeyValueList(List<(ReadOnlyMemory<byte> Name, ReadOnlyMemory<byte> Value)> entries)
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
