using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Shared.Types;

public class RequestHeader : IRequestHeader
{
    private readonly Request _request;

    private readonly KeyValueList _headers, _query;

    private readonly RequestTarget _target;

    private HttpProtocol _protocol;

    public RequestMethod Method { get; private set; }

    public ReadOnlyMemory<byte> Path => _request.Source.Path;

    public IRequestTarget Target => _target;

    public HttpProtocol Protocol => _protocol;

    public ReadOnlyMemory<byte> Version => _request.Source.Version;

    public IKeyValueList Headers => _headers;

    public IKeyValueList Query => _query;

    public RequestHeader(Request request)
    {
        _request = request;

        _headers = new KeyValueList(request.Source.Headers);
        _query = new KeyValueList(request.Source.QueryParameters);

        _target = new RequestTarget();
    }

    public void Apply()
    {
        _target.Apply(Path);

        _protocol = new(_request.Source.Version);

        // todo: bug in Glyph11?
        Span<byte> trimBy = [10];

        Method = new(_request.Source.Method.TrimStart(trimBy));
    }

}
