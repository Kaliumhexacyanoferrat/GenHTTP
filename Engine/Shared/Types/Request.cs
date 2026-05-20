using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using Glyph11.Protocol;

namespace GenHTTP.Engine.Shared.Types;

public sealed class Request : IRequest
{
    private static readonly ReadOnlyMemory<byte> HostHeader = "Host"u8.ToArray();

    private bool _bodyObtained;
    
    private IRequestHeader? _retainedHeader;

    private IServer? _server;

    private IEndPoint? _endPoint;

    private readonly RequestHeader _header;

    private readonly RequestProperties _properties;
    
    private readonly ResponseBuilder _response = new();

    private bool _resetRequired = true;

    #region Get-/Setters

    public BinaryRequest Source { get; }
    
    public IServer Server => _server ?? throw new InvalidOperationException("Server property has not been initialized");

    public IEndPoint EndPoint => _endPoint ?? throw new InvalidOperationException("EndPoint property has not been initialized");

    public IRequestProperties Properties => _properties;

    public IRequestHeader Header
    {
        get
        {
            if (!_bodyObtained)
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
        if (_bodyObtained)
        {
            throw new InvalidOperationException("Request body can only be fetched once.");
        }

        if (headerAccess == HeaderAccess.Retain)
        {
            _retainedHeader = new RetainedRequestHeader(_header);
        }

        _bodyObtained = true;

        // todo
        return null;
    }

    public void Apply(IServer server, IEndPoint endPoint)
    {
        _server = server;
        _endPoint = endPoint;

        _header.Apply();

        _properties.Clear();

        _bodyObtained = false;
        _retainedHeader = null;
    }
    
    public void Reset()
    {
        Source.Clear();
        
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
    
    #endregion

}
