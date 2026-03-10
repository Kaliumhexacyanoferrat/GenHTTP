using GenHTTP.Api.Protocol.Raw;

namespace GenHTTP.Api.Protocol;

public interface IResponse
{

    IRawResponse Raw { get; }

    IResponseBuilder Rebuild();

}
