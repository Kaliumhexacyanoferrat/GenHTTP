using GenHTTP.Api.Protocol.Raw;

namespace GenHTTP.Api.Protocol;

public interface IResponse
{

    IRawResponse Raw { get; }

    IResponseContent? Content { get; }

    IResponseBuilder Rebuild();

}
