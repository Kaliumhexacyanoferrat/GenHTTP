using GenHTTP.Api.Protocol;

using Glyph = Glyph11.Protocol;

namespace GenHTTP.Engine.Shared.Types;

public sealed class KeyValueList(Glyph.KeyValueList source) : IKeyValueList
{

    public int Count => source.Count;

    public KeyValuePair<ByteString, ByteString> this[int index]
    {
        get
        {
            var kv = source[index];
            
            return new KeyValuePair<ByteString, ByteString>(new(kv.Key), new(kv.Value));
        }
    }

}
