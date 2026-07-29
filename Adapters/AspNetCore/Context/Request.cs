using System.IO.Pipelines;
using System.Net;
using System.Security.Cryptography.X509Certificates;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Engine.Shared.Types;

using Microsoft.AspNetCore.Http.Features;

namespace GenHTTP.Adapters.AspNetCore.Context;

/// <summary>
/// Adapts an in-flight ASP.NET Core request (an <see cref="IFeatureCollection"/>) to the
/// engine-agnostic <see cref="IRequest"/> contract.
/// </summary>
public sealed class Request : IRequest
{
    private readonly RequestHeader _header = new();

    private readonly RequestBody _body = new();

    private readonly ClientConnection _client = new();

    private readonly PropertyBag _properties = new();

    private readonly ResponseBuilder _response = new();

    private IServer? _server;

    private IEndPoint? _endPoint;

    private IFeatureCollection? _features;

    // Whether the header is still allowed to be accessed after the body has been loaded.
    private bool _headerRetained;

    private IRequestBody? _wrappedBody;

    private Func<IRequestBody, IRequestBody>? _bodyWrapper;

    private bool _bodyLoaded;

    private bool _resetRequired = true;

    // Populated once the response has been upgraded, see SetUpgraded() below.
    private Stream? _upgradedStream;

    private PipeWriter? _upgradedWriter;

    private PipeReader? _upgradedReader;

    #region Get-/Setters

    public IServer Server => _server ?? throw new InvalidOperationException("Server property has not been initialized");

    public IEndPoint EndPoint => _endPoint ?? throw new InvalidOperationException("EndPoint property has not been initialized");

    public IClientConnection Client => _client;

    public IPropertyBag Properties => _properties;

    internal IFeatureCollection Features => _features ?? throw new InvalidOperationException("Features property has not been initialized");

    internal Stream? UpgradedStream => _upgradedStream;

    internal PipeWriter? UpgradedWriter => _upgradedWriter;

    public IRequestHeader Header
    {
        get
        {
            if (!_bodyLoaded || _headerRetained)
            {
                return _header;
            }

            throw new InvalidOperationException("Header information can no longer be accessed");
        }
    }

    #endregion

    #region Initialization

    public void Apply(IServer server, IEndPoint endPoint, IFeatureCollection features, IPAddress? remoteAddress, X509Certificate2? clientCertificate)
    {
        _server = server;
        _endPoint = endPoint;
        _features = features;

        _header.Apply(features.GetRequiredFeature<IHttpRequestFeature>());

        _client.Apply(remoteAddress, endPoint.Secure ? ClientProtocol.Https : ClientProtocol.Http, clientCertificate);

        _properties.Clear();

        _bodyLoaded = false;
        _headerRetained = false;

        _wrappedBody = null;
        _bodyWrapper = null;

        _upgradedStream = null;
        _upgradedWriter = null;
        _upgradedReader = null;
    }

    #endregion

    #region Functionality

    public IRequestBody? GetBody(HeaderAccess headerAccess = HeaderAccess.Retain)
    {
        if (_bodyLoaded)
        {
            throw new InvalidOperationException("Request body can only be fetched once.");
        }

        var headers = Header.Headers;

        _headerRetained = headerAccess == HeaderAccess.Retain;

        ulong? length = null;

        if (headers.GetEntry(KnownHeaders.ContentLength) is { } contentLength)
        {
            if (!ulong.TryParse(contentLength.Bytes.Span, out var parsed))
            {
                throw new ProviderException(ResponseStatus.BadRequest, "Content-Length header has an invalid value");
            }

            length = parsed;
        }

        var hasBody = length is not null || headers.ContainsKey(KnownHeaders.TransferEncoding);

        _bodyLoaded = true;

        if (!hasBody)
        {
            return null;
        }

        var requestFeature = Features.GetRequiredFeature<IHttpRequestFeature>();

        _body.Apply(requestFeature.Body, length);

        if (_bodyWrapper != null)
        {
            return _wrappedBody = _bodyWrapper(_body);
        }

        return _body;
    }

    public void WrapBody(Func<IRequestBody, IRequestBody> wrapper) => _bodyWrapper = wrapper;

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

    public PipeReader Upgrade()
        => _upgradedReader ?? throw new InvalidOperationException("The connection has not been upgraded yet");

    // Called by ResponseWriter once the connection has been upgraded, exposing the resulting
    // duplex stream both to Upgrade() and to ClientContext's Stream/Writer.
    internal void SetUpgraded(Stream stream)
    {
        _upgradedStream = stream;
        _upgradedWriter = PipeWriter.Create(stream);
        _upgradedReader = PipeReader.Create(stream);
    }

    public void Reset()
    {
        _response.Reset();

        _client.Reset();

        if (_wrappedBody is IDisposable disposableWrappedBody)
        {
            disposableWrappedBody.Dispose();
        }

        _wrappedBody = null;
        _bodyWrapper = null;

        _body.Reset();

        _upgradedStream = null;
        _upgradedWriter = null;
        _upgradedReader = null;

        _resetRequired = true;
    }

    #endregion

}
