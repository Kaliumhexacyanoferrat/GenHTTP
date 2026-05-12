using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol.Raw;

namespace GenHTTP.Api.Protocol;

public interface IRequest
{

    IServer Server { get; }

    IEndPoint EndPoint { get; }

    IRawRequest Raw { get; }

    IKeyValueList Headers { get; }

    IKeyValueList Query { get; }

    IRequestBody? Body { get; }

    RequestMethod Method { get; }

    string Host { get; }

    IResponseBuilder Respond();

}
