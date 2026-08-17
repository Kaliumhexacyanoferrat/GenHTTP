using System.Security.Cryptography.X509Certificates;

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
    /// UDP port for the whole server, so several endpoints asking for HTTP/3 would each want their
    /// own and only the first could have it - refused here rather than silently honouring one.
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
    /// Adds the QUIC listener for the endpoint serving HTTP/3. Needs a secure endpoint - QUIC
    /// carries TLS 1.3 and has no cleartext mode - and takes its port, which is what a browser
    /// assumes when an Alt-Svc advertisement names none of its own.
    /// </summary>
    private ServerConfig WithQuic(ServerConfig serverConfig)
    {
        var endPoint = _quicEndPoint!;

        if (endPoint is not SecureEndPoint quicEndPoint)
        {
            _logger.LogWarning("HTTP/3 was requested on port {Port}, which is not bound with a certificate; QUIC has no cleartext mode, so no listener was started.", endPoint.Port);
            return serverConfig;
        }

        if (quicEndPoint.Files is not { } files)
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
            clientCaPem: quicEndPoint.ClientCaPem);

        RegisterQuicHosts(quicEndPoint);

        // Built once here, not in the QuicHandle below - that runs per accepted connection, and
        // these two never change. Nghttp3Options is ngtcp2's own record; Http3Options is
        // what the caller sets, and this is where the two meet.
        _h3Options = new Nghttp3Options
        {
            QpackDynamicTableCapacity = _engineOptions.Http3.QpackDynamicTableCapacity,
            QpackBlockedStreams = _engineOptions.Http3.QpackBlockedStreams,
        };

        return serverConfig with
        {
            Udp = serverConfig.Udp ?? new UdpOptions(),
            Quic = new QuicOptions
            {
                Port = quicEndPoint.Port,
                ConnectionFactory = _quicEngine.CreateFactory(),
            },
        };
    }

    /// <summary>
    /// Registers the certificates this endpoint serves by name, so one UDP port can answer for
    /// several hosts.
    /// </summary>
    /// <remarks>
    /// Before the factory is built, and that is not a style preference: the table is read during
    /// handshakes on whichever reactor a connection landed on, with no lock, so ioxide refuses a
    /// host added once the engine is serving.
    ///
    /// A host without files is skipped rather than refused. ngtcp2 loads PEM by path and takes
    /// nothing else, and the engine will not write a private key out on anyone's behalf - so such
    /// a name serves HTTP/1.1 and HTTP/2 and is simply not offered over HTTP/3, which is the rule
    /// the default certificate already follows. Said out loud, because the alternative is a name
    /// that works on one port and quietly gets the wrong certificate on another.
    /// </remarks>
    private void RegisterQuicHosts(SecureEndPoint endPoint)
    {
        foreach (var host in endPoint.Hosts)
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

            try
            {
                _quicEngine!.AddHost(host.Host, files.Certificate, files.Key);
            }
            catch (Exception e) when (e is InvalidOperationException or ArgumentException)
            {
                // One unusable name is not worth losing the listener over - the default still
                // answers it, and every other name still gets its own certificate.
                _logger.LogError(e, "Could not serve {Host} over HTTP/3 on port {Port}; that name will be answered with the default certificate.",
                    host.Host, endPoint.Port);
            }
        }
    }

    /// <summary>
    /// Drops the QUIC engine. Nothing was written for it, so nothing is cleaned up.
    /// </summary>
    private void DisposeQuic()
    {
        _quicEngine?.Dispose();
        _quicEngine = null;
        _h3Options = null;
    }
}
