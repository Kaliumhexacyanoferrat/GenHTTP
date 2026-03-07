namespace GenHTTP.Api.Protocol.Raw;

public interface IRawResponse
{

    int StatusCode { get; }

    ReadOnlyMemory<byte> StatusPhrase { get; }

    IRawKeyValueList Headers { get; }

    IResponseContent? Content { get; }

}
