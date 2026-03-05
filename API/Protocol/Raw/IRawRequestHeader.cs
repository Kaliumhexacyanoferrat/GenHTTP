namespace GenHTTP.Api.Protocol.Raw;

public interface IRawRequestHeader
{

    ReadOnlyMemory<byte> Method { get; }

    ReadOnlyMemory<byte> Path { get; }

    IRawRequestTarget Target { get; }

    IRawKeyValueList Query { get; }

    ReadOnlyMemory<byte> Version { get; }

    IRawKeyValueList Headers { get; }

}
