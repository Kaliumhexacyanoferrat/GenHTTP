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
/// Adapts an in-flight ASP.NET Core request (represented by an <see cref="IFeatureCollection"/>)
/// to the engine agnostic <see cref="IRequest"/> contract.
/// </summary>
/// <remarks>
/// Clone of <c>GenHTTP.Engine.Shared.Types.Request</c>, adapted to the ASP.NET Core feature based
/// request model instead of the Glyph11-parsed <c>BinaryRequest</c> the Internal/Ioxide
/// engines are built around - see <see cref="RequestHeader"/> and <see cref="RequestBody"/>
/// for the parts that actually differ. Public: consumed by <c>GenHTTP.Engine.Kestrel</c> as well
/// as this package's own <c>Mapping.Bridge</c>.
/// </remarks>
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

    // Unlike the Glyph11-backed engines, our RequestHeader never aliases a reusable wire
    // buffer (its ByteStrings are independently allocated in RequestHeader.Apply()), so
    // there is nothing to snapshot for HeaderAccess.Retain - just whether it's still
    // allowed to be accessed after the body has been loaded.
    private bool _headerRetained;

    private IRequestBody? _wrappedBody;

    private Func<IRequestBody, IRequestBody>? _bodyWrapper;

    private bool _bodyLoaded;

    private bool _resetRequired = true;

    // Populated once the response has been upgraded (see Upgrade() below and
    // ResponseWriter, which drives IHttpUpgradeFeature.UpgradeAsync() before any
    // upgrade-mode body is written).
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

    /// <summary>
    /// Called by <see cref="ResponseWriter"/> once <see cref="IHttpUpgradeFeature.UpgradeAsync"/>
    /// has completed, exposing the resulting duplex stream both to <see cref="Upgrade"/>
    /// (used by e.g. the websocket module) and to <see cref="ClientContext"/>'s
    /// <c>Stream</c>/<c>Writer</c> (used by response sinks writing the upgraded body).
    /// </summary>
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
