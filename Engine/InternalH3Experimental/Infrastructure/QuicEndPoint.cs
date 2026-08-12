using System.Net;
using System.Net.Quic;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.InternalH3Experimental.Protocol;
using GenHTTP.Engine.Shared.Infrastructure;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Engine.InternalH3Experimental.Infrastructure;

/// <summary>
/// A QUIC listener serving HTTP/3.
/// </summary>
/// <remarks>
/// Does not derive from the Internal engine's EndPoint, which creates a TCP socket in its base
/// class and hands each connection over as a Stream. QUIC has neither.
/// </remarks>
internal sealed class QuicEndPoint : IEndPoint
{
    private readonly IServer _server;

    private readonly ILogger _logger;

    private readonly EndPointConfiguration _configuration;

    private readonly CancellationTokenSource _shutdown = new();

    private QuicListener? _listener;

    private Task? _accepting;

    internal QuicEndPoint(IServer server, EndPointConfiguration configuration)
    {
        _server = server;
        _configuration = configuration;
        _logger = server.Logging.CreateLogger<QuicEndPoint>();

        Address = configuration.Address;
        Port = configuration.Port;
        DualStack = configuration.DualStack;
    }

    public IPAddress? Address { get; }

    public ushort Port { get; }

    public bool DualStack { get; }

    // QUIC has no cleartext mode: TLS 1.3 is inside the transport.
    public bool Secure => true;

    internal async ValueTask StartAsync()
    {
        if (!QuicListener.IsSupported)
        {
            throw new NotSupportedException(
                "QUIC is not available on this system. libmsquic must be present and TLS 1.3 supported. "
                + "See https://learn.microsoft.com/dotnet/fundamentals/networking/quic/quic-overview");
        }

        if (_configuration.Security is null)
        {
            throw new InvalidOperationException("An HTTP/3 endpoint requires a certificate; bind it with one.");
        }

        X509Certificate2 certificate = _configuration.Security.CertificateProvider.Provide(null)
                                      ?? throw new InvalidOperationException("The certificate provider did not supply a certificate for the HTTP/3 endpoint.");

        IPAddress address = Address ?? (DualStack ? IPAddress.IPv6Any : IPAddress.Any);

        _listener = await QuicListener.ListenAsync(new QuicListenerOptions
        {
            ListenEndPoint = new IPEndPoint(address, Port),
            ApplicationProtocols = [SslApplicationProtocol.Http3],
            ConnectionOptionsCallback = (_, _, _) => ValueTask.FromResult(new QuicServerConnectionOptions
            {
                DefaultStreamErrorCode = 0x010c,   // H3_REQUEST_CANCELLED
                DefaultCloseErrorCode = 0x0100,    // H3_NO_ERROR
                ServerAuthenticationOptions = new SslServerAuthenticationOptions
                {
                    ApplicationProtocols = [SslApplicationProtocol.Http3],
                    ServerCertificate = certificate,
                },
            }),
        });

        _logger.LogInformation("Listening on {Address}:{Port} (HTTP/3 over QUIC)", address, Port);

        _accepting = Task.Run(AcceptAsync);
    }

    private async Task AcceptAsync()
    {
        try
        {
            while (!_shutdown.IsCancellationRequested)
            {
                QuicConnection connection = await _listener!.AcceptConnectionAsync(_shutdown.Token);

                _ = ServeAsync(connection);
            }
        }
        catch (Exception e)
        {
            if (!_shutdown.IsCancellationRequested)
            {
                _logger.LogError(e, "Failed to accept incoming connection");
            }
        }
    }

    private async Task ServeAsync(QuicConnection connection)
    {
        try
        {
            await H3Connection.ServeAsync(connection, _server, this, _logger, _shutdown.Token);
        }
        catch (Exception e)
        {
            _logger.LogDebug(e, "Connection ended");
        }
    }

    public void Dispose()
    {
        _shutdown.Cancel();

        if (_listener is not null)
        {
            _listener.DisposeAsync().AsTask().GetAwaiter().GetResult();
            _listener = null;
        }

        _shutdown.Dispose();
    }
}
