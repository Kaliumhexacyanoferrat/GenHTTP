namespace GenHTTP.Api.Protocol;

public interface IKeyValueList
{

    int Count { get; }

    KeyValuePair<ByteString, ByteString> this[int index] { get; }

}
