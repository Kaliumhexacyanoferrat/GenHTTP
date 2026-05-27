using GenHTTP.Api.Protocol;

using Glyph = Glyph11.Protocol;

namespace GenHTTP.Engine.Shared.Types;

public sealed class KeyValueList(Glyph.KeyValueList source) : IKeyValueList
{

    public int Count => source.Count;

    public KeyValuePair<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>> this[int index] => source[index];

}
