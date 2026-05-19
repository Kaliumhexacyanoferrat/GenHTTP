using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Api.Protocol.Raw;


public interface IRawRequest
{

    IServer Server { get; }

    IEndPoint EndPoint { get; }

    IRequestProperties Properties { get; }

    IRawRequestHeader Header { get;}

    IRawRequestBody? GetBody(HeaderAccess headerAccess);

    // todo: wrap body (e.g. content decoding)

}

public enum HeaderAccess
{
    Retain,
    Release
}
