namespace GenHTTP.Api.Protocol;

public interface IRequestHeader
{

    HttpProtocol Protocol { get; }

    RequestMethod Method { get; }

    ByteString Path { get; }

    IRequestTarget Target { get; }

    IKeyValueList Query { get; }

    IKeyValueList Headers { get; }

}
