namespace GenHTTP.Api.Protocol;

public interface IRequestHeader
{

    RequestMethod Method { get; }

    ReadOnlyMemory<byte> Path { get; }

    ReadOnlyMemory<byte> Version { get; }

    IRequestTarget Target { get; }

    IKeyValueList Query { get; }

    IKeyValueList Headers { get; }

}
