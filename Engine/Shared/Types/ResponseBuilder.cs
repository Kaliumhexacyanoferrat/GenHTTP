using GenHTTP.Api.Protocol;
using GenHTTP.Api.Protocol.Raw;
using GenHTTP.Engine.Shared.Types.Raw;

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

    public IResponseBuilder Status(ResponseStatus status)
    {
        _raw.Status(status);
        return this;
    }

    public IResponseBuilder Header(string name, string value)
    {
        _raw.Header(name.GetMemory(), value.GetMemory());
        return this;
    }

    public IRawResponseBuilder Raw() => _raw;

    public IResponse Build() => _response;

    public void Reset() => _response.Reset();

}
