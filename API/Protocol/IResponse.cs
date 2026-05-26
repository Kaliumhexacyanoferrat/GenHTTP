namespace GenHTTP.Api.Protocol;

public interface IResponse
{

    ResponseStatus Status { get; }

    Connection Mode { get; }

    IKeyValueList Headers { get; }

    IResponseContent? Content { get; }
    
    IResponseBuilder Rebuild();

}
