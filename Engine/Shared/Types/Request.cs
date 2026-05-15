using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Protocol.Raw;

using GenHTTP.Engine.Shared.Types.Raw;

using Glyph11.Protocol;

namespace GenHTTP.Engine.Shared.Types;

public sealed class Request : IRequest
{
    private static readonly ReadOnlyMemory<byte> HostHeader = "Host"u8.ToArray();

    private readonly RawRequest _raw = new();

    private readonly ResponseBuilder _response = new();

    private readonly IKeyValueList _header;

    private readonly IKeyValueList _query;

    private bool _resetRequired = true;

    public IServer Server => _raw.Server;

    public IEndPoint EndPoint => _raw.EndPoint;

    public IRawRequest Raw => _raw;

    public IKeyValueList Headers => _header;

    public IKeyValueList Query => _query;

    public IRequestBody? Body { get; }

    public BinaryRequest Source => _raw.Source;

    public RequestMethod Method => _raw.Header.Method;

    public string Host => _header.GetValue(HostHeader) ?? throw new InvalidOperationException("Request is missing mandatory host header");

    public Request()
    {
        _header = new KeyValueList(_raw.Header.Headers);
        _query = new KeyValueList(_raw.Header.Query);
    }

    public void Apply(IServer server, IEndPoint endPoint)
    {
        _raw.Apply(server, endPoint);
    }

    public void Reset()
    {
        _raw.Source.Clear();
        _response.Reset();

        _resetRequired = true;
    }

    public IResponseBuilder Respond()
    {
        if (!_resetRequired)
        {
            _response.Reset();
        }
        else
        {
            _resetRequired = false;
        }

        return _response;
    }

}
