using System.Diagnostics;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Engine.Shared.Hosting;

public sealed class ServerHost : IServerHost
{
    private readonly IServerBuilder _Builder;

    #region Get-/Setters

    public IServer? Instance { get; private set; }

    #endregion

    #region  Initialization

    public ServerHost(IServerBuilder builder)
    {
        _Builder = builder;
    }

    #endregion

    #region Builder facade

    public IServerHost Backlog(ushort backlog)
    {
        _Builder.Backlog(backlog);
        return this;
    }

    public IServerHost Bind(IPAddress address, ushort port)
    {
        _Builder.Bind(address, port);
        return this;
    }

    public IServerHost Bind(IPAddress address, ushort port, X509Certificate2 certificate)
    {
        _Builder.Bind(address, port, certificate);
        return this;
    }

    public IServerHost Bind(IPAddress address, ushort port, X509Certificate2 certificate, SslProtocols protocols)
    {
        _Builder.Bind(address, port, certificate, protocols);
        return this;
    }

    public IServerHost Bind(IPAddress address, ushort port, ICertificateProvider certificateProvider)
    {
        _Builder.Bind(address, port, certificateProvider);
        return this;
    }

    public IServerHost Bind(IPAddress address, ushort port, ICertificateProvider certificateProvider, SslProtocols protocols)
    {
        _Builder.Bind(address, port, certificateProvider, protocols);
        return this;
    }

    public IServerHost Companion(IServerCompanion companion)
    {
        _Builder.Companion(companion);
        return this;
    }

    public IServerHost Console()
    {
        _Builder.Console();
        return this;
    }

    public IServerHost Development(bool developmentMode = true)
    {
        _Builder.Development(developmentMode);
        return this;
    }

    public IServerHost Port(ushort port)
    {
        _Builder.Port(port);
        return this;
    }

    public IServerHost RequestMemoryLimit(uint limit)
    {
        _Builder.RequestMemoryLimit(limit);
        return this;
    }

    public IServerHost RequestReadTimeout(TimeSpan timeout)
    {
        _Builder.RequestReadTimeout(timeout);
        return this;
    }

    public IServerHost Handler(IHandler handler)
    {
        _Builder.Handler(handler);
        return this;
    }

    public IServerHost TransferBufferSize(uint bufferSize)
    {
        _Builder.TransferBufferSize(bufferSize);
        return this;
    }

    public IServerHost Add(IConcernBuilder concern)
    {
        _Builder.Add(concern);
        return this;
    }

    public IServer Build() => _Builder.Build();

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

            var companion = Instance?.Companion;

            if (companion is not null)
            {
                companion.OnServerError(ServerErrorScope.General, null, e);
            }
            else
            {
                System.Console.WriteLine(e);
            }

            return -1;
        }
    }

    public async ValueTask<IServerHost> StartAsync()
    {
        await StopAsync();

        Instance = Build();

        await Instance.StartAsync();

        return this;
    }

    public async ValueTask<IServerHost> StopAsync()
    {
        if (Instance != null)
        {
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
