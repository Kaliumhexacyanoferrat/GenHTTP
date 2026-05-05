using GenHTTP.Api.Protocol.Raw;

namespace GenHTTP.Engine.Shared.Types.Raw;

public class RawRequestHeader : IRawRequestHeader
{
    private readonly RawRequest _request;

    private readonly RawKeyValueList _headers, _query;

    private readonly RawRequestTarget _target;

    public ReadOnlyMemory<byte> Method => _request.Source.Method;

    public ReadOnlyMemory<byte> Path => _request.Source.Path;

    public IRawRequestTarget Target => _target;

    public ReadOnlyMemory<byte> Version => _request.Source.Version;

    public IRawKeyValueList Headers => _headers;

    public IRawKeyValueList Query => _query;

    public RawRequestHeader(RawRequest request)
    {
        _request = request;

        _headers = new RawKeyValueList(request.Source.Headers);
        _query = new RawKeyValueList(request.Source.QueryParameters);

        _target = new RawRequestTarget();
    }

    public void Apply()
    {
        _target.Apply(Path);
    }

}
