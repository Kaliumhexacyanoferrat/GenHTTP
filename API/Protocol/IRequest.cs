using GenHTTP.Api.Protocol.Raw;

namespace GenHTTP.Api.Protocol;

public interface IRequest
{

    IRawRequest Raw { get; }

    RequestMethod Method { get; }

    IRequestBody? Body { get; }

    IResponseBuilder Respond();

}
