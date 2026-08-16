using System.Security.Cryptography.X509Certificates;

using ioxide;
using ioxide.ngtcp2;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Engine.Ioxide.Hosting;

/// <summary>
/// The QUIC listener that carries HTTP/3, alongside the TCP one.
/// </summary>
public sealed partial class IoxideServer
{
    private QuicEngine? _quic;

    private IoxideEndPoint? _quicEndPoint;

    /// <summary>
    /// Adds the QUIC listener for the endpoint bound with <c>enableQuic</c>.
    /// </summary>
    /// <remarks>
    /// QUIC carries TLS 1.3 and has no cleartext mode, so this needs a secure endpoint - the
    /// certificate bound there is the one it serves. The UDP port is the endpoint's own port, which
    /// is what a browser assumes when it reads an Alt-Svc advertisement naming no port of its own.
    /// </remarks>
    private ServerConfig WithQuic(ServerConfig cfg, IoxideEndPoint endPoint)
    {
        if (!_secure.TryGetValue(endPoint.Port, out var security))
        {
            _logger.LogWarning("HTTP/3 was requested on port {Port}, which is not bound with a certificate; QUIC has no cleartext mode, so no listener was started.", endPoint.Port);
            return cfg;
        }

        if (!TryResolveQuicCertificate(security, endPoint.Port, out var certPath, out var keyPath))
        {
            return cfg;
        }

        _quic = new QuicEngine(certPath, keyPath, alpn: ["h3"],
            clientCaPemPath: _options.MutualTls.ClientCaPath,
            requireClientCertificate: RequiresClientCertificate(security));

        _quicEndPoint = endPoint;

        return cfg with
        {
            Udp = cfg.Udp ?? new UdpOptions(),
            Quic = new QuicOptions
            {
                Port = endPoint.Port,
                ConnectionFactory = _quic.CreateFactory(),
            },
        };
    }

    /// <summary>
    /// The PEM files ngtcp2 loads. Configured, or HTTP/3 does not start.
    /// </summary>
    /// <remarks>
    /// ngtcp2 takes paths rather than a certificate object, so serving HTTP/3 needs PEM on disk.
    /// That is the C layer's contract and this honours it rather than working around it: the engine
    /// will not write a private key out on your behalf, because a key it creates is a key it has to
    /// choose a location and a lifetime for, and one written to a temporary directory outlives any
    /// shutdown that skips cleanup.
    /// </remarks>
    private bool TryResolveQuicCertificate(Shared.Infrastructure.SecurityConfiguration security, ushort port,
        out string certPath, out string keyPath)
    {
        certPath = keyPath = string.Empty;

        if (_options.Http3.CertificatePath is not { } configuredCert || _options.Http3.KeyPath is not { } configuredKey)
        {
            throw new InvalidOperationException(
                $"Port {port} serves HTTP/3, which needs a PEM certificate and key on disk - ngtcp2 loads them by path. "
                + "Set IoxideOptions.Http3.CertificatePath and Http3.KeyPath to the same certificate bound to that endpoint.");
        }

        if (!File.Exists(configuredCert) || !File.Exists(configuredKey))
        {
            _logger.LogError("The configured HTTP/3 certificate or key does not exist ({Certificate}, {Key}); no listener was started.", configuredCert, configuredKey);
            return false;
        }

        WarnIfNotTheBoundCertificate(configuredCert, security, port);

        certPath = configuredCert;
        keyPath = configuredKey;
        return true;
    }

    /// <summary>
    /// Warns when the configured PEM is not the certificate bound to this endpoint.
    /// </summary>
    /// <remarks>
    /// These paths exist to hand ngtcp2 a file rather than to give HTTP/3 an identity of its own,
    /// and nothing stops them doing the latter: the port would then answer as one host over TCP and
    /// another over QUIC. A browser moving from HTTP/1.1 to HTTP/3 by an Alt-Svc header expects the
    /// alternative to present a certificate valid for the ORIGIN (RFC 7838 3.1), so it would refuse
    /// the upgrade - or worse, not notice.
    ///
    /// <para>Compared by leaf thumbprint, so a file carrying a fuller chain than the bound
    /// certificate is not flagged. A warning rather than a refusal: someone may be deliberately
    /// serving a different certificate, and this is not the place to decide they cannot.</para>
    /// </remarks>
    private void WarnIfNotTheBoundCertificate(string configuredCert, Shared.Infrastructure.SecurityConfiguration security, ushort port)
    {
        if (security.CertificateProvider.Provide(null) is not { } bound)
        {
            return;
        }

        try
        {
            // From the PEM text, not CreateFromPemFile - that one wants a private key alongside
            // the certificate and throws on a certificate-only file, which is what this usually is.
            using var configured = X509Certificate2.CreateFromPem(File.ReadAllText(configuredCert));

            if (!string.Equals(configured.Thumbprint, bound.Thumbprint, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning(
                    "The HTTP/3 certificate configured for port {Port} ({ConfiguredSubject}) is not the one bound to that endpoint ({BoundSubject}). "
                    + "The port will answer as one host over TCP and another over QUIC, and a browser following an Alt-Svc advertisement expects them to match.",
                    port, configured.Subject, bound.Subject);
            }
        }
        catch (Exception e)
        {
            // Only the comparison failed; ngtcp2 will report a certificate it cannot load itself.
            _logger.LogDebug(e, "Could not compare the configured HTTP/3 certificate at {Path} with the bound one", configuredCert);
        }
    }

    /// <summary>
    /// Drops the QUIC engine. Nothing was written for it, so nothing is cleaned up.
    /// </summary>
    private void DisposeQuic()
    {
        _quic?.Dispose();
        _quic = null;
    }
}
