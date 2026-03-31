using GenHTTP.Api.Protocol;
using GenHTTP.Api.Protocol.Raw;

namespace GenHTTP.Engine.Shared.Types;

public sealed class KeyValueList(IRawKeyValueList source) : IKeyValueList
{

    public string? GetValue(string key) => source.GetEntry(key.GetMemory())?.GetString();

    public string? GetValue(ReadOnlyMemory<byte> key) => GetValue(key.GetString());

}
