using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol.Raw;

namespace GenHTTP.Api.Protocol;

public interface IResponseBuilder : IBuilder<IResponse>
{

    IRawResponseBuilder Raw();

}
