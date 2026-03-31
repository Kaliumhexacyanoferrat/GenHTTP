using GenHTTP.Api.Protocol.Raw;

namespace GenHTTP.Api.Protocol;

public interface IRequest
{

    IRawRequest Raw { get; }

    RequestMethod Method { get; }

    string Host { get; }

    IRequestBody? Body { get; }

    string? GetHeader(string header);

    IResponseBuilder Respond();

}
