using GenHTTP.Api.Protocol.Raw;
using Glyph11.Protocol;

namespace GenHTTP.Engine.Shared.Types;

public sealed class RawKeyValueList(KeyValueList source) : IRawKeyValueList
{

    public int Count => source.Count;

    public KeyValuePair<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>> this[int index] => source[index];

}
