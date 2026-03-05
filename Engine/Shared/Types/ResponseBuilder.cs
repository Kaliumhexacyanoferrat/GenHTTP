using GenHTTP.Api.Protocol;
using GenHTTP.Api.Protocol.Raw;

namespace GenHTTP.Engine.Shared.Types;

public class ResponseBuilder : IResponseBuilder
{
    private readonly Response _response;

    private readonly RawResponseBuilder _raw;

    public ResponseBuilder()
    {
        _response = new(this);
        _raw = new(_response, this);
    }

    public IRawResponseBuilder Raw() => _raw;

    public IResponse Build() => _response;

    public void Reset() => _response.Reset();

}
