namespace GenHTTP.Api.Protocol.Raw;

public interface IRawResponse
{

    ResponseStatus Status { get; }

    Connection Mode { get; }

    IRawKeyValueList Headers { get; }

    IResponseContent? Content { get; }

}
