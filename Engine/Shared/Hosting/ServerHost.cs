using System.Diagnostics;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Engine.Shared.Hosting;

public sealed class ServerHost : IServerHost
{
    private readonly IServerBuilder _builder;

    #region Get-/Setters

    public IServer? Instance { get; private set; }

    #endregion

    #region  Initialization

    public ServerHost(IServerBuilder builder)
    {
        _builder = builder;
    }

    #endregion

    #region Builder facade

    public IServerHost Bind(IPAddress? address, ushort port, bool dualStack = true)
    {
        _builder.Bind(address, port, dualStack);
        return this;
    }

    public IServerHost Bind(IPAddress? address, ushort port, X509Certificate2 certificate, SslProtocols protocols = SslProtocols.Tls12 | SslProtocols.Tls13, ICertificateValidator? certificateValidator = null, bool enableQuic = false, bool dualStack = true)
    {
        _builder.Bind(address, port, certificate, protocols, certificateValidator, enableQuic, dualStack);
        return this;
    }

    public IServerHost Bind(IPAddress? address, ushort port, ICertificateProvider certificateProvider, SslProtocols protocols = SslProtocols.Tls12 | SslProtocols.Tls13, ICertificateValidator? certificateValidator = null, bool enableQuic = false, bool dualStack = true)
    {
        _builder.Bind(address, port, certificateProvider, protocols, certificateValidator, enableQuic, dualStack);
        return this;
    }

    public IServerHost Development(bool developmentMode = true)
    {
        _builder.Development(developmentMode);
        return this;
    }

    public IServerHost Logging(ILoggerFactory loggerFactory, bool logRequests = true)
    {
        _builder.Logging(loggerFactory, logRequests);
        return this;
    }

    public IServerHost Port(ushort port)
    {
        _builder.Port(port);
        return this;
    }

    public IServerHost Handler(IHandler handler)
    {
        _builder.Handler(handler);
        return this;
    }

    public IServerHost Add(IConcernBuilder concern)
    {
        _builder.Add(concern);
        return this;
    }

    public IServer Build() => _builder.Build();

    #endregion

    #region Functionality

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
                Instance.Logging.CreateLogger<ServerHost>().LogCritical(e, "Server stopped unexpectedly");
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

        Instance = Build();
        
        var logger = Instance.Logging.CreateLogger<ServerHost>();

        logger.LogInformation("Starting server ...");
        
        await Instance.StartAsync();

        logger.LogInformation("Server has started");

        return this;
    }

    public async ValueTask<IServerHost> StopAsync()
    {
        if (Instance != null)
        {
            Instance.Logging.CreateLogger<ServerHost>().LogInformation("Server is shutting down ...");

            await Instance.DisposeAsync();
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

    #endregion

}
