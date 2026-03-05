using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Api.Protocol.Raw;

public interface IRawResponseBuilder : IBuilder<IResponse>
{

    IRawResponseBuilder Status(int code, ReadOnlyMemory<byte> phrase);

    IRawResponseBuilder Header(ReadOnlyMemory<byte> name, ReadOnlyMemory<byte> value);

    IRawResponseBuilder Content(IResponseContent? content);

    IResponseBuilder Unraw();

}
