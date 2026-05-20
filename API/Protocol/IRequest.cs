using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Api.Protocol;

public interface IRequest
{

    IServer Server { get; }

    IEndPoint EndPoint { get; }

    IRequestProperties Properties { get; }

    IRequestHeader Header { get;}

    IRequestBody? GetBody(HeaderAccess headerAccess);

    IResponseBuilder Respond();
    
    // todo: wrap body (e.g. content decoding)

}

public enum HeaderAccess
{
    Retain,
    Release
}
