namespace GenHTTP.Api.Protocol.Raw;

public interface IRawResponse
{

    ResponseStatus Status { get; }

    IRawKeyValueList Headers { get; }

    IResponseContent? Content { get; }

}
