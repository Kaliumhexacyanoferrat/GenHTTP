namespace GenHTTP.Api.Protocol;

public interface IRequestHeader
{

    HttpProtocol Protocol { get; }

    RequestMethod Method { get; }

    ByteString Path { get; }

    IRequestTarget Target { get; }

    IRequestQuery Query { get; }

    IRequestHeaders Headers { get; }

}
