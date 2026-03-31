using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol.Raw;

namespace GenHTTP.Api.Protocol;

public interface IResponseBuilder : IBuilder<IResponse>
{

    IResponseBuilder Status(ResponseStatus status);

    IResponseBuilder Header(string name, string value);

    IRawResponseBuilder Raw();

}
