using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Api.Protocol;

public interface IResponseBuilder : IBuilder<IResponse>
{

    IResponseBuilder Status(ResponseStatus code);

    IResponseBuilder Connection(Connection mode);

    IResponseBuilder Header(ReadOnlyMemory<byte> name, ReadOnlyMemory<byte> value);
    
    IResponseBuilder Header(string name, string value);

    IResponseBuilder Content(IResponseContent? content);

}
