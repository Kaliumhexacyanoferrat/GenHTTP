namespace GenHTTP.Api.Protocol;

public interface IKeyValueList
{

    int Count { get; }

    KeyValuePair<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>> this[int index] { get; }

}
