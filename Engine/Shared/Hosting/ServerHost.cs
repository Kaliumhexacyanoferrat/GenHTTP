using System.Diagnostics;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Shared.Infrastructure;
using GenHTTP.Engine.Shared.Infrastructure.Logging;
using GenHTTP.Engine.Shared.Security;

using GenHTTP.Modules.ErrorHandling;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace GenHTTP.Engine.Shared.Hosting;

public abstract class ServerHost : IServerHost
{
    private readonly List<IConcernBuilder> _concerns = [];

    private readonly List<EndPointConfiguration> _endPoints = [];

    private bool _development;

    private IHandler? _handler;

    private ushort _port = 8080;
    
    private ILoggerFactory? _loggerFactory;

    private bool _logRequests = true;

    private bool _autoLogging = true;
    
    #region Get-/Setters

    public IServer? Instance { get; private set; }
    
    #endregion

    #region Functionality
    
    public IServerHost Bind(IPAddress? address, ushort port, bool dualStack = true)
    {
        _endPoints.Add(new EndPointConfiguration(address, port, dualStack, null, false));
        return this;
    }

    public IServerHost Bind(IPAddress? address, ushort port, X509Certificate2 certificate, SslProtocols protocols = SslProtocols.Tls12 | SslProtocols.Tls13, ICertificateValidator? certificateValidator = null, bool enableQuic = false, bool dualStack = true) => Bind(address, port, new SimpleCertificateProvider(certificate), protocols, certificateValidator, enableQuic, dualStack);

    public IServerHost Bind(IPAddress? address, ushort port, ICertificateProvider certificateProvider, SslProtocols protocols = SslProtocols.Tls12 | SslProtocols.Tls13, ICertificateValidator? certificateValidator = null, bool enableQuic = false, bool dualStack = true)
    {
        _endPoints.Add(new EndPointConfiguration(address, port, dualStack, new SecurityConfiguration(certificateProvider, protocols, certificateValidator), enableQuic));
        return this;
    }

    public IServerHost Development(bool developmentMode = true)
    {
        _development = developmentMode;
        return this;
    }

    public IServerHost Logging(ILoggerFactory loggerFactory, bool logRequests = true)
    {
        _loggerFactory = loggerFactory;
        _logRequests = logRequests && !(loggerFactory is NullLoggerFactory);
        _autoLogging = false;

        return this;
    }

    public IServerHost Port(ushort port)
    {
        if (port == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(port));
        }

        _port = port;
        return this;
    }

    public IServerHost Handler(IHandler handler)
    {
        _handler = handler;
        return this;
    }

    public IServerHost Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);
        return this;
    }
    
    public async ValueTask<int> RunAsync()
    {
        try
        {
            var waitEvent = new ManualResetEvent(false);

            AppDomain.CurrentDomain.ProcessExit += (_, _) =>
            {
                waitEvent.Set();
            };

            await StartAsync();

            try
            {
                waitEvent.WaitOne();
            }
            finally
            {
                await StopAsync();
            }

            return 0;
        }
        catch (Exception e)
        {
            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }

            if (Instance is not null)
            {
                Instance.Logging.CreateLogger<IServerHost>().LogCritical(e, "Server stopped unexpectedly");
            }
            else
            {
                Console.WriteLine(e);
            }

            return -1;
        }
    }

    public async ValueTask<IServerHost> StartAsync()
    {
        await StopAsync();

        Instance = CreateInstance();

        var logger = Instance.Logging.CreateLogger<IServerHost>();

        logger.LogInformation("Starting server ...");

        await Instance.StartAsync();

        logger.LogInformation("Server has started");

        return this;
    }

    public async ValueTask<IServerHost> StopAsync()
    {
        if (Instance != null)
        {
            Instance.Logging.CreateLogger<IServerHost>().LogInformation("Server is shutting down ...");
            
            await Instance.DisposeAsync();
        }

        if (_autoLogging && _loggerFactory is not null)
        {
            _loggerFactory.Dispose();
            _loggerFactory = null;
        }

        Instance = null;

        return this;
    }

    public async ValueTask<IServerHost> RestartAsync()
    {
        await StopAsync();
        await StartAsync();

        return this;
    }
    
    protected abstract IServer Build(ServerConfiguration config, IHandler handler);
    
    private IServer CreateInstance()
    {
        if (_handler is null)
        {
            throw new BuilderMissingPropertyException("Handler");
        }

        var endpoints = new List<EndPointConfiguration>(_endPoints);

        if (endpoints.Count == 0)
        {
            endpoints.Add(new EndPointConfiguration(null, _port, true, null, false));
        }
        
        var logging = _loggerFactory ?? CreateDefaultLoggerFactory();

        var config = new ServerConfiguration(_development, endpoints, logging);

        var concerns = new List<IConcernBuilder> { ErrorHandler.Default() };

        concerns.AddRange(_concerns);

        if (_logRequests)
        {
            concerns.Add(new RequestLoggingConcernBuilder());
        }

        var handler = new CoreRouter(_handler, concerns);

        return Build(config, handler);
    }

    private static ILoggerFactory CreateDefaultLoggerFactory() => LoggerFactory.Create(builder => builder.AddConsole());

    #endregion

}
