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
    private readonly List<IConcernBuilder> _Concerns = [];
    private readonly List<EndPointConfiguration> _EndPoints = [];

    private ushort _Backlog = 1024;

    private IServerCompanion? _Companion;

    private bool _Development;

    private IHandler? _Handler;

    private ushort _Port = 8080;

    private uint _RequestMemoryLimit = 1 * 1024 * 1024; // 1 MB

    private TimeSpan _RequestReadTimeout = TimeSpan.FromSeconds(10);
    private uint _TransferBufferSize = 65 * 1024; // 65 KB

    #region Content

    public IServerBuilder Handler(IHandler handler)
    {
        _Handler = handler;
        return this;
    }

    #endregion

    #region Extensibility

    public IServerBuilder Add(IConcernBuilder concern)
    {
        _Concerns.Add(concern);
        return this;
    }

    #endregion

    #region Infrastructure

    public IServerBuilder Console()
    {
        _Companion = new ConsoleCompanion();
        return this;
    }

    public IServerBuilder Companion(IServerCompanion companion)
    {
        _Companion = companion;
        return this;
    }

    public IServerBuilder Development(bool developmentMode = true)
    {
        _Development = developmentMode;
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

        _Port = port;
        return this;
    }

    public IServerBuilder Bind(IPAddress? address, ushort port, bool dualStack = true)
    {
        _EndPoints.Add(new EndPointConfiguration(address, port, dualStack, null, false));
        return this;
    }

    public IServerBuilder Bind(IPAddress? address, ushort port, X509Certificate2 certificate, SslProtocols protocols = SslProtocols.Tls12 | SslProtocols.Tls13, ICertificateValidator? certificateValidator = null, bool enableQuic = false, bool dualStack = true) => Bind(address, port, new SimpleCertificateProvider(certificate), protocols, certificateValidator, enableQuic, dualStack);

    public IServerBuilder Bind(IPAddress? address, ushort port, ICertificateProvider certificateProvider, SslProtocols protocols = SslProtocols.Tls12 | SslProtocols.Tls13, ICertificateValidator? certificateValidator = null, bool enableQuic = false, bool dualStack = true)
    {
        _EndPoints.Add(new EndPointConfiguration(address, port, dualStack, new SecurityConfiguration(certificateProvider, protocols, certificateValidator), enableQuic));
        return this;
    }

    #endregion

    #region Network settings

    public IServerBuilder Backlog(ushort backlog)
    {
        _Backlog = backlog;
        return this;
    }

    public IServerBuilder RequestReadTimeout(TimeSpan timeout)
    {
        _RequestReadTimeout = timeout;
        return this;
    }

    public IServerBuilder RequestMemoryLimit(uint limit)
    {
        _RequestMemoryLimit = limit;
        return this;
    }

    public IServerBuilder TransferBufferSize(uint bufferSize)
    {
        _TransferBufferSize = bufferSize;
        return this;
    }

    #endregion

    #region Builder

    public IServer Build()
    {
        if (_Handler is null)
        {
            throw new BuilderMissingPropertyException("Handler");
        }

        var network = new NetworkConfiguration(_RequestReadTimeout, _RequestMemoryLimit, _TransferBufferSize, _Backlog);

        var endpoints = new List<EndPointConfiguration>(_EndPoints);

        if (endpoints.Count == 0)
        {
            endpoints.Add(new EndPointConfiguration(null, _Port, true, null, false));
        }

        var config = new ServerConfiguration(_Development, endpoints, network);

        var concerns = new[]
        {
            ErrorHandler.Default()
        }.Concat(_Concerns);

        var handler = new CoreRouter(_Handler, concerns);

        return Build(_Companion, config, handler);
    }

    protected abstract IServer Build(IServerCompanion? companion, ServerConfiguration config, IHandler handler);

    #endregion

}
