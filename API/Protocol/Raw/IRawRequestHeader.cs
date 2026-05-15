namespace GenHTTP.Api.Protocol.Raw;

public interface IRawRequestHeader
{

    RequestMethod Method { get; }

    ReadOnlyMemory<byte> Path { get; }

    ReadOnlyMemory<byte> Version { get; }

    IRawRequestTarget Target { get; }

    IRawKeyValueList Query { get; }

    IRawKeyValueList Headers { get; }

}
