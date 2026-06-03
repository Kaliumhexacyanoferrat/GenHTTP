using System.IO.Pipelines;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using Glyph11.Protocol;

namespace GenHTTP.Engine.Shared.Types;

public sealed class Request : IRequest
{
    private SequencePosition _bodyStart;

    private IRequestHeader? _retainedHeader;

    private IServer? _server;

    private IEndPoint? _endPoint;

    private PipeReader? _reader;
    
    private readonly RequestHeader _header;

    private readonly RequestProperties _properties;

    private readonly ResponseBuilder _response = new();

    private readonly RequestBody _body;

    private bool _bodyLoaded;
    
    private bool _resetRequired = true;

    #region Get-/Setters

    public BinaryRequest Source { get; }

    public PipeReader Reader => _reader ?? throw new InvalidOperationException("Reader property has not been initialized");

    public IServer Server => _server ?? throw new InvalidOperationException("Server property has not been initialized");

    public IEndPoint EndPoint => _endPoint ?? throw new InvalidOperationException("EndPoint property has not been initialized");

    public IRequestProperties Properties => _properties;

    public IRequestHeader Header
    {
        get
        {
            if (!_bodyLoaded)
            {
                if (_retainedHeader != null)
                {
                    return _retainedHeader;
                }

                return _header;
            }

            if (_retainedHeader == null)
            {
                throw new InvalidOperationException("Header information can no longer be accessed");
            }

            return _retainedHeader;
        }
    }

    #endregion

    #region Initialization

    public Request()
    {
        Source = new();

        _header = new(this);
        _body = new(this);
        _properties = new RequestProperties();
    }

    #endregion

    #region Functionality

    public IRequestBody? GetBody(HeaderAccess headerAccess)
    {
        if (_bodyLoaded)
        {
            throw new InvalidOperationException("Request body can only be fetched once.");
        }

        _bodyLoaded = true;

        var headers = Header.Headers;

        if (headerAccess == HeaderAccess.Retain && _retainedHeader == null)
        {
            _retainedHeader = new RetainedRequestHeader(_header);
        }

        var hasBody = false;

        if (headers.ContainsKey(KnownHeaders.ContentLength) || headers.ContainsKey(KnownHeaders.TransferEncoding))
        {
            hasBody = true;
            _body.Apply(Reader);
        }

        Reader.AdvanceTo(_bodyStart);

        return (hasBody) ? _body : null;
    }

    public void Apply(IServer server, IEndPoint endPoint, PipeReader reader, SequencePosition bodyStart)
    {
        _server = server;
        _endPoint = endPoint;
        _reader = reader;

        _header.Apply();

        _properties.Clear();

        _bodyLoaded = false;
        _bodyStart = bodyStart;
        _retainedHeader = null;
    }

    public void Apply(IServer server)
    {
        _server = server;

        _header.Apply();

        _properties.Clear();
        
        _bodyLoaded = false;
        _bodyStart = default;
        _retainedHeader = null;
    }
    
    public void Reset()
    {
        Source.Clear();

        _response.Reset();
        _body.Reset();

        _resetRequired = true;
    }

    public ValueTask DrainAsync()
    {
        if (_bodyLoaded)
        {
            return _body.DrainAsync();
        }

        GetBody(HeaderAccess.Release);
        
        if (_bodyLoaded)
        {
            return _body.DrainAsync();
        }
        
        return default;
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

    #endregion

}
