using System.IO.Pipelines;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using Glyph11.Protocol;

namespace GenHTTP.Engine.Shared.Types;

public sealed class Request : IRequest
{
    private IRequestHeader? _retainedHeader;

    private IServer? _server;

    private IEndPoint? _endPoint;

    private readonly RequestHeader _header;

    private readonly ClientConnection _client = new();

    private readonly PropertyBag _properties;

    private readonly ResponseBuilder _response = new();

    private readonly RequestBody _body;

    private bool _resetRequired = true;

    #region Get-/Setters

    public BinaryRequest Source { get; }

    public IServer Server => _server ?? throw new InvalidOperationException("Server property has not been initialized");

    public IEndPoint EndPoint => _endPoint ?? throw new InvalidOperationException("EndPoint property has not been initialized");

    public IClientConnection Client => _client;

    public IPropertyBag Properties => _properties;

    public IRequestHeader Header
    {
        get
        {
            if (!_body.Opened)
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

    public bool HasBody => _body.Present;

    #endregion

    #region Initialization

    public Request()
    {
        Source = new();

        _header = new(this);
        _body = new(_header);
        _properties = new PropertyBag();
    }

    #endregion

    #region Functionality

    public IRequestBody? GetBody(HeaderAccess headerAccess = HeaderAccess.Retain)
    {
        if (_body.Opened)
        {
            throw new InvalidOperationException("Request body can only be fetched once.");
        }

        if (headerAccess == HeaderAccess.Retain && _retainedHeader == null)
        {
            _retainedHeader = new RetainedRequestHeader(_header);
        }

        return _body.Open();
    }

    /// <summary>
    /// Prepares this instance to serve a request that has been parsed from the given connection.
    /// </summary>
    /// <param name="writer">The writer of the connection, used to answer a client that expects a 100 (Continue); null, if the engine does not support interim responses</param>
    public void Apply(IServer server, IEndPoint endPoint, PipeReader reader, SequencePosition bodyStart, IPAddress? remoteAddress, X509Certificate? clientCertificate, PipeWriter? writer)
    {
        _server = server;
        _endPoint = endPoint;

        _header.Apply();

        _client.Apply(remoteAddress, endPoint.Secure ? ClientProtocol.Https : ClientProtocol.Http, clientCertificate);

        _properties.Clear();

        _retainedHeader = null;

        _body.Apply(reader, bodyStart, writer);
    }

    public void Apply(IServer server)
    {
        _server = server;

        _header.Apply();

        _client.Apply(null, null, null);

        _properties.Clear();

        _retainedHeader = null;

        _body.Apply();
    }

    public void Reset()
    {
        Source.Clear();

        _response.Reset();
        _body.Reset();

        _client.Reset();

        _resetRequired = true;
    }

    /// <summary>
    /// Invoked by the engine right before the final response to this
    /// request is written to the connection.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ResponseStarting() => _body.ResponseStarting();

    /// <summary>
    /// Invoked by the engine after the response has been written to
    /// consume whatever is left of the request body.
    /// </summary>
    /// <returns>true, if the connection can be used to serve another request</returns>
    public ValueTask<bool> FinishAsync() => _body.FinishAsync();

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

    public void WrapBody(Func<IRequestBody, IRequestBody> wrapper) => _body.Wrap(wrapper);

    public PipeReader Upgrade()
    {
        // the upgraded connection outlives the pipe buffer the header was parsed from
        if (!_body.Opened)
        {
            _retainedHeader ??= new RetainedRequestHeader(_header);
        }

        return _body.Upgrade();
    }

    #endregion

}
