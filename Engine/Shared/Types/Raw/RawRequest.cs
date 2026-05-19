using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Protocol.Raw;

using Glyph11.Protocol;

namespace GenHTTP.Engine.Shared.Types.Raw;

public sealed class RawRequest : IRawRequest
{
    private bool _bodyObtained;

    private readonly RawRequestHeader _header;

    private readonly RequestProperties _properties;

    private IRawRequestHeader? _retainedHeader;

    private IServer? _server;

    private IEndPoint? _endPoint;

    #region Get-/Setters

    public IServer Server => _server ?? throw new InvalidOperationException("Server property has not been initialized");

    public IEndPoint EndPoint => _endPoint ?? throw new InvalidOperationException("EndPoint property has not been initialized");

    public IRequestProperties Properties => _properties;

    public IRawRequestHeader Header
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

    internal BinaryRequest Source { get; }

    public RawRequest()
    {
        Source = new();

        _header = new(this);
        _properties = new RequestProperties();
    }

    #endregion

    #region Functionality

    public IRawRequestBody? GetBody(HeaderAccess headerAccess)
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

    #endregion

}
