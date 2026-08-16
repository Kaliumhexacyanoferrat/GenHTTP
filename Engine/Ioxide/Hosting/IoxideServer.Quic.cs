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

    // Only set when a certificate had to be written out; a user-supplied path is never touched.
    private string? _exportedCertPath;

    private string? _exportedKeyPath;

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
    /// The PEM files ngtcp2 loads: the ones configured, or the bound certificate written out.
    /// </summary>
    /// <remarks>
    /// ngtcp2 takes paths, not a certificate object, so one of the two has to happen. A configured
    /// path is used as it is and nothing is written. Otherwise the endpoint's certificate is
    /// exported to a file this user alone can read, which does put a private key on disk for the
    /// lifetime of the process - so a deployment holding PEM files should name them through
    /// <see cref="IoxideHttp3Options.CertificatePath"/> and skip this entirely.
    /// </remarks>
    private bool TryResolveQuicCertificate(Shared.Infrastructure.SecurityConfiguration security, ushort port,
        out string certPath, out string keyPath)
    {
        if (_options.Http3.CertificatePath is { } configuredCert && _options.Http3.KeyPath is { } configuredKey)
        {
            if (!File.Exists(configuredCert) || !File.Exists(configuredKey))
            {
                _logger.LogError("The configured HTTP/3 certificate or key does not exist ({Certificate}, {Key}); no listener was started.", configuredCert, configuredKey);
                certPath = keyPath = string.Empty;
                return false;
            }

            WarnIfNotTheBoundCertificate(configuredCert, security, port);

            certPath = configuredCert;
            keyPath = configuredKey;
            return true;
        }

        if (security.CertificateProvider.Provide(null) is not { } certificate)
        {
            _logger.LogWarning("No default certificate for port {Port}; no HTTP/3 listener was started.", port);
            certPath = keyPath = string.Empty;
            return false;
        }

        var directory = Directory.CreateTempSubdirectory("genhttp-ioxide-");

        // Owner-only, set before anything is written: the key must never exist world-readable, not
        // even for the moment between creating the file and tightening it. The engine only runs on
        // Linux (io_uring), but the file APIs are cross-platform and the analyzer checks them.
        if (!OperatingSystem.IsWindows())
        {
            directory.UnixFileMode = UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute;
        }

        _exportedCertPath = Path.Combine(directory.FullName, "quic.crt");
        _exportedKeyPath = Path.Combine(directory.FullName, "quic.key");

        WriteOwnerOnly(_exportedCertPath, certificate.ExportCertificatePem());
        WriteOwnerOnly(_exportedKeyPath, ExportKeyPem(certificate));

        _logger.LogInformation("Exported the certificate bound to port {Port} to {Directory} for the HTTP/3 listener; set Http3CertificatePath to avoid writing a key to disk.", port, directory.FullName);

        certPath = _exportedCertPath;
        keyPath = _exportedKeyPath;
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

    private static void WriteOwnerOnly(string path, string content)
    {
        var options = new FileStreamOptions
        {
            Mode = FileMode.CreateNew,
            Access = FileAccess.Write,
        };

        if (!OperatingSystem.IsWindows())
        {
            options.UnixCreateMode = UnixFileMode.UserRead | UnixFileMode.UserWrite;
        }

        using var stream = new FileStream(path, options);
        using var writer = new StreamWriter(stream);

        writer.Write(content);
    }

    /// <summary>
    /// Drops the QUIC engine and anything that was written out for it.
    /// </summary>
    private void DisposeQuic()
    {
        _quic?.Dispose();
        _quic = null;

        if (_exportedCertPath is null)
        {
            return;
        }

        try
        {
            Directory.Delete(Path.GetDirectoryName(_exportedCertPath)!, recursive: true);
        }
        catch (IOException e)
        {
            // Best effort. A leftover key in a temp directory is worth a line in the log, but not a
            // failed shutdown - it is owner-only and the directory name is unique to this process.
            _logger.LogWarning(e, "Could not remove the exported HTTP/3 certificate at {Path}", _exportedCertPath);
        }

        _exportedCertPath = null;
        _exportedKeyPath = null;
    }
}
