using System.Text;

using GenHTTP.Api.Protocol;
using GenHTTP.Api.Protocol.Raw;
using GenHTTP.Engine.Shared.Types.Raw;
using GenHTTP.Engine.Shared.Utilities;

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

    private RequestMethod? _method;

    public IRawRequest Raw => _raw;

    public IKeyValueList Headers => _header;

    public IKeyValueList Query => _query;

    public IRequestBody? Body { get; }

    public BinaryRequest Source => _raw.Source;

    public RequestMethod Method
    {
        get
        {
            if (_method == null)
            {
                var m = _raw.Header.Method.Span;

                _method = m.Length switch
                {
                    3 when AsciiComparer.EqualsIgnoreCase(m, "GET"u8) => RequestMethod.Get,
                    4 when AsciiComparer.EqualsIgnoreCase(m, "POST"u8) => RequestMethod.Post,
                    3 when AsciiComparer.EqualsIgnoreCase(m, "PUT"u8) => RequestMethod.Put,
                    6 when AsciiComparer.EqualsIgnoreCase(m, "DELETE"u8) => RequestMethod.Delete,
                    4 when AsciiComparer.EqualsIgnoreCase(m, "HEAD"u8) => RequestMethod.Head,
                    7 when AsciiComparer.EqualsIgnoreCase(m, "OPTIONS"u8) => RequestMethod.Options,
                    5 when AsciiComparer.EqualsIgnoreCase(m, "PATCH"u8) => RequestMethod.Patch,
                    5 when AsciiComparer.EqualsIgnoreCase(m, "TRACE"u8) => RequestMethod.Trace,
                    7 when AsciiComparer.EqualsIgnoreCase(m, "CONNECT"u8) => RequestMethod.Connect,
                    _ => RequestMethod.Other
                };
            }

            return _method.Value;
        }
    }

    public string Host => _header.GetValue(HostHeader) ?? throw new InvalidOperationException("Request is missing mandatory host header");

    public Request()
    {
        _header = new KeyValueList(_raw.Header.Headers);
        _query = new KeyValueList(_raw.Header.Query);
    }

    public void Apply()
    {
        _raw.Apply();
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
