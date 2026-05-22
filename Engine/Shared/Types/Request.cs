using System.IO.Pipelines;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using Glyph11.Protocol;

namespace GenHTTP.Engine.Shared.Types;

public sealed class Request : IRequest
{
    private IRequestHeader? _retainedHeader;

    private IServer? _server;

    private IEndPoint? _endPoint;

    private PipeReader? _reader;
    
    private readonly RequestHeader _header;

    private readonly RequestProperties _properties;

    private readonly ResponseBuilder _response = new();
    
    private RequestBody? _body;

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
            if (_body == null)
            {
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
        _properties = new RequestProperties();
    }

    #endregion

    #region Functionality

    public IRequestBody? GetBody(HeaderAccess headerAccess)
    {
        if (_body != null)
        {
            throw new InvalidOperationException("Request body can only be fetched once.");
        }

        var headers = Header.Headers;

        if (!headers.ContainsKey(KnownHeaders.ContentLength) && !headers.ContainsKey(KnownHeaders.TransferEncoding))
        {
            // todo: is this enough?
            return null;
        }

        if (headerAccess == HeaderAccess.Retain)
        {
            _retainedHeader = new RetainedRequestHeader(_header);
        }

        return _body = new RequestBody(this, Reader);
    }

    public void Apply(IServer server, IEndPoint endPoint, PipeReader reader)
    {
        _server = server;
        _endPoint = endPoint;
        _reader = reader;

        _header.Apply();

        _properties.Clear();

        _body = null;
        _retainedHeader = null;
    }

    public void Reset()
    {
        Source.Clear();

        _response.Reset();

        _resetRequired = true;
        
    }

    public ValueTask DrainAsync()
    {
        if (_body != null)
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
