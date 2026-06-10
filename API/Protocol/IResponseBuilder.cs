using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Api.Protocol;

public interface IResponseBuilder : IBuilder<IResponse>
{

    IResponseBuilder Status(ResponseStatus status);

    IResponseBuilder Connection(Connection mode);

    IResponseBuilder Header(ByteString name, ByteString value);
    
    IResponseBuilder Header(string name, string value);

    IResponseBuilder Content(IResponseContent? content);

}
