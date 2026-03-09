using GenHTTP.Api.Protocol;
using GenHTTP.Api.Protocol.Raw;
using GenHTTP.Engine.Shared.Types.Raw;

namespace GenHTTP.Engine.Shared.Types;

public class Response(IResponseBuilder builder) : IResponse
{
    private readonly RawResponse _raw = new();

    public IRawResponse Raw => _raw;

    public IResponseBuilder Rebuild() => builder;

    public RawResponse Source => _raw;

    public void Reset() => _raw.Reset();

}
