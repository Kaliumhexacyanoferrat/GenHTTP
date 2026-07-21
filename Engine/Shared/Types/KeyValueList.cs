using GenHTTP.Api.Protocol;
using Glyph = Glyph11.Protocol;

namespace GenHTTP.Engine.Shared.Types;

public sealed class KeyValueList(Glyph.KeyValueList source) : IRequestHeaders, IRequestQuery
{

    public int Count => source.Count;

    public KeyValuePair<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>> GetMemoryEntry(int index) => source[index];

}
