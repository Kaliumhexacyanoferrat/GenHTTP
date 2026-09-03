using GenHTTP.Engine.Ioxide.Infrastructure.Endpoints;

using ioxide;
using ioxide.nghttp3;
using ioxide.ngtcp2;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Engine.Ioxide.Infrastructure;

/// <summary>
/// The QUIC listener that carries HTTP/3, alongside the TCP one.
/// </summary>
public sealed partial class Server
{
    private QuicEngine? _quicEngine;

    /// <summary>The endpoint serving HTTP/3, or null. Resolved in the constructor.</summary>
    private readonly EndPoint? _quicEndPoint;

    private Nghttp3Options? _h3Options;

    /// <summary>
    /// The endpoint that wants a QUIC listener, or none. At most one: the transport binds a single
    /// UDP port for the whole server.
    /// </summary>
    private EndPoint? ResolveQuicEndPoint()
    {
        var quicEndPoints = _endPoints.Where(e => e.Protocols.HasFlag(Protocols.Http3)).ToList();

        if (quicEndPoints.Count > 1)
        {
            throw new NotSupportedException(
                $"The ioxide engine binds one QUIC listener, but HTTP/3 was requested on ports {string.Join(", ", quicEndPoints.Select(e => e.Port))}. "
                + "Name the protocols per port (ProtocolsByPort) so only one of them serves HTTP/3.");
        }

        return quicEndPoints.Count == 1 ? quicEndPoints[0] : null;
    }

    /// <summary>
    /// Adds the QUIC listener. Needs a secure endpoint - QUIC carries TLS 1.3 and has no cleartext
    /// mode - and takes its port, which is what a browser assumes when Alt-Svc names none.
    /// </summary>
    private ServerConfig WithQuic(ServerConfig serverConfig)
    {
        var endPoint = _quicEndPoint!;

        if (endPoint is not SecureEndPoint quicEndPoint)
        {
            _logger.LogWarning("HTTP/3 was requested on port {Port}, which is not bound with a certificate; QUIC has no cleartext mode, so no listener was started.", endPoint.Port);
            return serverConfig;
        }

        if (quicEndPoint.CertificateFiles is not { } files)
        {
            throw new NotSupportedException(
                $"Port {quicEndPoint.Port} serves HTTP/3, but its binding supplies only an in-memory certificate. "
                + $"ngtcp2 loads PEM by path and the engine will not write a private key out on your behalf - bind "
                + $"the port with an {nameof(IFileCertificateProvider)} that names the certificate and key as files.");
        }

        if (!File.Exists(files.Certificate) || !File.Exists(files.Key))
        {
            _logger.LogError("The HTTP/3 certificate or key for port {Port} does not exist ({Certificate}, {Key}); no listener was started.",
                quicEndPoint.Port, files.Certificate, files.Key);

            return serverConfig;
        }

        _quicEngine = new QuicEngine(files.Certificate, files.Key, alpn: ["h3"],
            clientCaPemPath: quicEndPoint.ClientCaPath,
            requireClientCertificate: quicEndPoint.RequireClientCertificate,
            clientCaPem: quicEndPoint.ClientCaPem,
            handshakeTimeoutMs: _engineOptions.Http3.HandshakeTimeoutMs);

        RegisterQuicHosts(quicEndPoint);

        _h3Options = new Nghttp3Options
        {
            QpackDynamicTableCapacity = _engineOptions.Http3.QpackDynamicTableCapacity,
            QpackBlockedStreams = _engineOptions.Http3.QpackBlockedStreams,
        };

        return serverConfig with
        {
            // The listener binds its own port below; Udp carries the socket tuning they share.
            Udp = (serverConfig.Udp ?? new UdpOptions()) with
            {
                SocketBufferBytes = _engineOptions.Http3.SocketBufferBytes,
            },
            Quic = new QuicOptions
            {
                Port = quicEndPoint.Port,
                ConnectionFactory = _quicEngine.CreateFactory(),

                IdleTimeoutMs = _engineOptions.Http3.IdleTimeoutMs,
                Routing = _engineOptions.Http3.Routing,
                PinMigratedPeers = _engineOptions.Http3.PinMigratedPeers,
            },
        };
    }

    /// <summary>
    /// Registers the certificates this endpoint serves by name, before the factory is built:
    /// the table is read during handshakes with no lock, so ioxide refuses a host added later.
    /// </summary>
    private void RegisterQuicHosts(SecureEndPoint endPoint)
    {
        foreach (var (host, certificate) in ResolveQuicHosts(endPoint, endPoint.Hosts))
        {
            try
            {
                _quicEngine!.AddHost(host, certificate.CertificatePath, certificate.KeyPath);
            }
            catch (Exception e) when (e is InvalidOperationException or ArgumentException)
            {
                // One unusable name is not worth losing the listener over.
                _logger.LogError(e, "Could not serve {Host} over HTTP/3 on port {Port}; that name will be answered with the default certificate.",
                    host, endPoint.Port);
            }
        }
    }

    /// <summary>
    /// The named certificates HTTP/3 can serve, as ngtcp2 takes them. A name without files - or
    /// with files that are not there - is dropped loudly, since the default certificate will then
    /// answer it and produce a name mismatch at the client.
    /// </summary>
    private Dictionary<string, QuicCertificate> ResolveQuicHosts(SecureEndPoint endPoint, IReadOnlyList<HostCertificate> hosts)
    {
        var resolved = new Dictionary<string, QuicCertificate>(hosts.Count, StringComparer.OrdinalIgnoreCase);

        foreach (var host in hosts)
        {
            if (host.Files is not { } files)
            {
                _logger.LogWarning("The certificate for {Host} on port {Port} is in-memory only, so HTTP/3 cannot serve it; clients asking for that name over QUIC get the default certificate. Bind it with an {Provider} to serve it here too.",
                    host.Host, endPoint.Port, nameof(IFileCertificateProvider));

                continue;
            }

            if (!File.Exists(files.Certificate) || !File.Exists(files.Key))
            {
                _logger.LogError("The HTTP/3 certificate or key for {Host} does not exist ({Certificate}, {Key}); that name was not registered.",
                    host.Host, files.Certificate, files.Key);

                continue;
            }

            resolved[host.Host] = new QuicCertificate(files.Certificate, files.Key);
        }

        return resolved;
    }

    /// <summary>
    /// The HTTP/3 half of <see cref="ReloadCertificates"/>. Default and names go together, since
    /// ngtcp2 publishes a generation with one atomic store, and nothing is published unless all of
    /// it loaded.
    /// </summary>
    private void ReloadQuicCertificates(SecureEndPoint endPoint, CertificateFiles? files, IReadOnlyList<HostCertificate> hosts)
    {
        if (_quicEngine is null)
        {
            return;
        }

        if (files is null || !File.Exists(files.Certificate) || !File.Exists(files.Key))
        {
            throw new InvalidOperationException(
                $"Port {endPoint.Port} serves HTTP/3, but its provider no longer names a certificate and key that exist "
                + "as files. The listener still serves the ones it had.");
        }

        _quicEngine.ReplaceCertificates(new QuicCertificate(files.Certificate, files.Key),
            ResolveQuicHosts(endPoint, hosts));
    }

    /// <summary>Drops the QUIC engine. Nothing was written for it, so nothing is cleaned up.</summary>
    private void DisposeQuic()
    {
        _quicEngine?.Dispose();
        _quicEngine = null;
        _h3Options = null;
    }
}
