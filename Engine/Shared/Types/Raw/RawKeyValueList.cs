using GenHTTP.Api.Protocol.Raw;
using Glyph = Glyph11.Protocol;

namespace GenHTTP.Engine.Shared.Types.Raw;

public sealed class RawKeyValueList(Glyph.KeyValueList source) : IRawKeyValueList
{

    public int Count => source.Count;

    public KeyValuePair<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>> this[int index] => source[index];

}
