using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Shared.Security;

using GenHTTP.Modules.ErrorHandling;

namespace GenHTTP.Engine.Shared.Infrastructure;

public abstract class ServerBuilder : IServerBuilder
{
    private readonly List<IConcernBuilder> _concerns = [];
    private readonly List<EndPointConfiguration> _endPoints = [];

    private ushort _backlog = 1024;

    private IServerCompanion? _companion;

    private bool _development;

    private IHandler? _handler;

    private ushort _port = 8080;

    private uint _requestMemoryLimit = 1 * 1024 * 1024; // 1 MB

    private TimeSpan _requestReadTimeout = TimeSpan.FromSeconds(10);
    private uint _transferBufferSize = 65 * 1024; // 65 KB

    #region Content

    public IServerBuilder Handler(IHandler handler)
    {
        _handler = handler;
        return this;
    }

    #endregion

    #region Extensibility

    public IServerBuilder Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);
        return this;
    }

    #endregion

    #region Infrastructure

    public IServerBuilder Console()
    {
        _companion = new ConsoleCompanion();
        return this;
    }

    public IServerBuilder Companion(IServerCompanion companion)
    {
        _companion = companion;
        return this;
    }

    public IServerBuilder Development(bool developmentMode = true)
    {
        _development = developmentMode;
        return this;
    }

    #endregion

    #region Binding

    public IServerBuilder Port(ushort port)
    {
        if (port == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(port));
        }

        _port = port;
        return this;
    }

    public IServerBuilder Bind(IPAddress? address, ushort port, bool dualStack = true)
    {
        _endPoints.Add(new EndPointConfiguration(address, port, dualStack, null, false));
        return this;
    }

    public IServerBuilder Bind(IPAddress? address, ushort port, X509Certificate2 certificate, SslProtocols protocols = SslProtocols.Tls12 | SslProtocols.Tls13, ICertificateValidator? certificateValidator = null, bool enableQuic = false, bool dualStack = true) => Bind(address, port, new SimpleCertificateProvider(certificate), protocols, certificateValidator, enableQuic, dualStack);

    public IServerBuilder Bind(IPAddress? address, ushort port, ICertificateProvider certificateProvider, SslProtocols protocols = SslProtocols.Tls12 | SslProtocols.Tls13, ICertificateValidator? certificateValidator = null, bool enableQuic = false, bool dualStack = true)
    {
        _endPoints.Add(new EndPointConfiguration(address, port, dualStack, new SecurityConfiguration(certificateProvider, protocols, certificateValidator), enableQuic));
        return this;
    }

    #endregion

    #region Network settings

    public IServerBuilder Backlog(ushort backlog)
    {
        _backlog = backlog;
        return this;
    }

    public IServerBuilder RequestReadTimeout(TimeSpan timeout)
    {
        _requestReadTimeout = timeout;
        return this;
    }

    public IServerBuilder RequestMemoryLimit(uint limit)
    {
        _requestMemoryLimit = limit;
        return this;
    }

    public IServerBuilder TransferBufferSize(uint bufferSize)
    {
        _transferBufferSize = bufferSize;
        return this;
    }

    #endregion

    #region Builder

    public IServer Build()
    {
        if (_handler is null)
        {
            throw new BuilderMissingPropertyException("Handler");
        }

        var network = new NetworkConfiguration(_requestReadTimeout, _requestMemoryLimit, _transferBufferSize, _backlog);

        var endpoints = new List<EndPointConfiguration>(_endPoints);

        if (endpoints.Count == 0)
        {
            endpoints.Add(new EndPointConfiguration(null, _port, true, null, false));
        }

        var config = new ServerConfiguration(_development, endpoints, network);

        var concerns = new[]
        {
            ErrorHandler.Default()
        }.Concat(_concerns);

        var handler = new CoreRouter(_handler, concerns);

        return Build(_companion, config, handler);
    }

    protected abstract IServer Build(IServerCompanion? companion, ServerConfiguration config, IHandler handler);

    #endregion

}
