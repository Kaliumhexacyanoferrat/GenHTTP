using System.Security.Cryptography.X509Certificates;

using GenHTTP.Engine.Shared.Infrastructure;

using ioxide.tls;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Engine.Ioxide.Hosting;

/// <summary>
/// TLS termination for the TCP endpoints - HTTP/1.1 and HTTP/2 both ride this.
/// </summary>
public sealed partial class IoxideServer
{
    /// <summary>
    /// The TLS options for every secure port whose provider yields a default (no-SNI) certificate.
    /// </summary>
    /// <remarks>
    /// Providers that select by SNI (unsupported here) return none and are skipped - the port stays
    /// advertised as secure, so secure-upgrade redirects still work, but its handshakes are refused.
    /// </remarks>
    private IEnumerable<KeyValuePair<ushort, TlsOptions>> ResolveTls()
    {
        foreach (var (port, security) in _secure)
        {
            if (security.CertificateProvider.Provide(null) is not { } certificate)
            {
                _logger.LogWarning("No default certificate for secure port {Port}; handshakes there will be refused (SNI selection is unsupported).", port);
                continue;
            }

            yield return new(port, new TlsOptions
            {
                CertificatePem = certificate.ExportCertificatePem(),
                KeyPem = ExportKeyPem(certificate),

                // Server preference, most preferred first: a client offering both gets HTTP/2, one
                // offering only http/1.1 is unaffected, and one offering neither continues without
                // an ALPN extension at all.
                Alpn = ProtocolsFor(port).HasFlag(IoxideProtocols.Http2) ? ["h2", "http/1.1"] : ["http/1.1"],

                ClientCaPath = _options.MutualTls.ClientCaPath,
                ClientCaPem = _options.MutualTls.ClientCaPem,
                RequireClientCertificate = RequiresClientCertificate(security),

                KernelTx = _kernelTx,
                KernelRx = _kernelRx
            });
        }
    }

    /// <summary>
    /// Whether a client offering no certificate is refused on this endpoint.
    /// </summary>
    /// <remarks>
    /// Either the engine says so for every endpoint, or the endpoint's own validator does. A
    /// validator that only wants to inspect what arrives leaves <c>RequireCertificate</c> false and
    /// still gets asked, because the CertificateRequest goes out either way.
    /// </remarks>
    private bool RequiresClientCertificate(SecurityConfiguration security)
        => _options.MutualTls.RequireClientCertificate || security.CertificateValidator?.RequireCertificate == true;

    /// <summary>
    /// Whether any endpoint asks for a client certificate at all.
    /// </summary>
    private bool MutualTlsConfigured
        => _options.MutualTls.ClientCaPath is not null || _options.MutualTls.ClientCaPem is not null
           || _options.MutualTls.RequireClientCertificate || _secure.Values.Any(s => s.CertificateValidator is not null);

    private static string ExportKeyPem(X509Certificate2 certificate)
        => certificate.GetRSAPrivateKey()?.ExportPkcs8PrivateKeyPem()
           ?? certificate.GetECDsaPrivateKey()?.ExportPkcs8PrivateKeyPem()
           ?? throw new InvalidOperationException("The certificate carries no exportable RSA or ECDSA private key.");
}
