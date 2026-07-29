using GenHTTP.Api.Protocol;

namespace GenHTTP.Adapters.AspNetCore.Context;

/// <summary>
/// A mutable list of key/value pairs used to expose Kestrel's flattened request
/// headers/query parameters as <see cref="IRequestHeaders"/>/<see cref="IRequestQuery"/>.
/// </summary>
/// <remarks>
/// Shared's <c>EditableKeyValueList</c> only implements the generic <see cref="IKeyValueList"/>
/// (it exists to build up <c>Response.Headers</c>), not these two request-side marker
/// interfaces, so it can't be reused here directly - this is the same storage shape,
/// just implementing both interfaces at once (mirroring Shared's Glyph11-backed
/// <c>KeyValueList</c>, which does the same for the other engines).
/// </remarks>
internal sealed class KeyValueList : IRequestHeaders, IRequestQuery
{
    private readonly List<KeyValuePair<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>>> _store = [];

    public int Count => _store.Count;

    public KeyValuePair<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>> GetMemoryEntry(int index) => _store[index];

    public void Add(ByteString key, ByteString value) => _store.Add(new(key.Bytes, value.Bytes));

    public void Clear() => _store.Clear();

}
