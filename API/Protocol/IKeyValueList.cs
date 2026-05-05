namespace GenHTTP.Api.Protocol;

public interface IKeyValueList
{

    string? GetValue(string key);

    string? GetValue(ReadOnlyMemory<byte> key);

}
