using GenHTTP.Engine.Ioxide.Infrastructure.Endpoints;

using ioxide;
using ioxide.nghttp3;
using ioxide.ngtcp2;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Engine.Ioxide.Infrastructure;

/// <summary>The QUIC listener, and the HTTP/3 stack above it.</summary>
public sealed partial class Server
{
    private QuicEngine? _quicEngine;

    private readonly EndPoint? _quicEndPoint;

    private Nghttp3Options? _h3Options;

    // The one endpoint serving HTTP/3; more than one is refused, since this engine binds a single QUIC listener.
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

    // Adds the QUIC listener, which needs a certificate named as files - QUIC has no cleartext mode.
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
            handshakeTimeoutMs: _engineOptions.Quic.HandshakeTimeoutMs);

        RegisterQuicHosts(quicEndPoint);

        _h3Options = new Nghttp3Options
        {
            QpackDynamicTableCapacity = _engineOptions.Http3.QpackDynamicTableCapacity,
            QpackBlockedStreams = _engineOptions.Http3.QpackBlockedStreams,
        };

        return serverConfig with
        {
            Udp = (serverConfig.Udp ?? new UdpOptions()) with
            {
                SocketBufferBytes = _engineOptions.Quic.SocketBufferBytes,
            },
            Quic = new QuicOptions
            {
                Port = quicEndPoint.Port,
                ConnectionFactory = _quicEngine.CreateFactory(),

                IdleTimeoutMs = _engineOptions.Quic.IdleTimeoutMs,
                Routing = _engineOptions.Quic.Routing,
                PinMigratedPeers = _engineOptions.Quic.PinMigratedPeers,
            },
        };
    }

    // Registers each name with ngtcp2, logging the ones it refuses rather than losing the listener.
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
                _logger.LogError(e, "Could not serve {Host} over HTTP/3 on port {Port}; that name will be answered with the default certificate.",
                    host, endPoint.Port);
            }
        }
    }

    // The names HTTP/3 can actually serve: those whose certificate exists on disk.
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

    // Swaps the QUIC certificates in one atomic publish, or throws having changed nothing.
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

    // Drops the QUIC engine. Nothing was written for it, so nothing is cleaned up.
    private void DisposeQuic()
    {
        _quicEngine?.Dispose();
        _quicEngine = null;
        _h3Options = null;
    }
}
