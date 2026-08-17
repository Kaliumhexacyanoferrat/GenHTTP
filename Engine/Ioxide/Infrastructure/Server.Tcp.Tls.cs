using System.Security.Cryptography.X509Certificates;

using GenHTTP.Engine.Ioxide.Infrastructure.Endpoints;
using GenHTTP.Engine.Shared.Infrastructure;

using ioxide.tls;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Engine.Ioxide.Infrastructure;

/// <summary>
/// TLS termination for the TCP endpoints - HTTP/1.1 and HTTP/2 both ride this.
/// </summary>
public sealed partial class Server
{
    /// <summary>
    /// The TLS options for every secure port whose provider yields a default (no-SNI) certificate.
    /// A provider selecting by SNI (unsupported here) returns none, leaving the port advertised as
    /// secure - so secure-upgrade redirects still work - but refusing its handshakes.
    /// </summary>
    private IEnumerable<KeyValuePair<ushort, TlsOptions>> ResolveTls()
    {
        foreach (var endPoint in SecureEndPoints)
        {
            var port = endPoint.Port;

            if (endPoint.Security!.CertificateProvider.Provide(null) is not { } certificate)
            {
                _logger.LogWarning("No default certificate for secure port {Port}; handshakes there will be refused (SNI selection is unsupported).", port);
                continue;
            }

            yield return new(port, new TlsOptions
            {
                CertificatePem = certificate.ExportCertificatePem(),
                KeyPem = ExportKeyPem(certificate),

                // Server preference, most preferred first. A client offering neither continues
                // without an ALPN extension at all.
                Alpn = endPoint.Protocols.HasFlag(Protocols.Http2) ? ["h2", "http/1.1"] : ["http/1.1"],

                ClientCaPath = _engineOptions.MutualTls.ClientCaPath,
                ClientCaPem = _engineOptions.MutualTls.ClientCaPem,
                RequireClientCertificate = RequiresClientCertificate(endPoint.Security),

                KernelTx = _engineOptions.Tcp.TxKernelTls,
                KernelRx = _engineOptions.Tcp.RxKernelTls
            });
        }
    }

    /// <summary>
    /// Whether a client offering no certificate is refused on this endpoint: either the engine says
    /// so for every endpoint, or the endpoint's own validator does. A validator that only wants to
    /// inspect what arrives still gets asked, because the CertificateRequest goes out either way.
    /// </summary>
    /// <remarks>
    /// Shared with the QUIC half, which asks the same question of ngtcp2 - mutual TLS is the one
    /// setting that means the same thing on both transports.
    /// </remarks>
    private bool RequiresClientCertificate(SecurityConfiguration security)
        => _engineOptions.MutualTls.RequireClientCertificate || security.CertificateValidator?.RequireCertificate == true;

    /// <summary>
    /// Whether any endpoint asks for a client certificate at all.
    /// </summary>
    private bool MutualTlsConfigured
        => _engineOptions.MutualTls.ClientCaPath is not null || _engineOptions.MutualTls.ClientCaPem is not null
           || _engineOptions.MutualTls.RequireClientCertificate
           || SecureEndPoints.Any(e => e.Security!.CertificateValidator is not null);

    /// <summary>The endpoints bound with a certificate - the ones TLS applies to.</summary>
    private IEnumerable<EndPoint> SecureEndPoints => _endPoints.Where(e => e.Security is not null);

    private static string ExportKeyPem(X509Certificate2 certificate)
        => certificate.GetRSAPrivateKey()?.ExportPkcs8PrivateKeyPem()
           ?? certificate.GetECDsaPrivateKey()?.ExportPkcs8PrivateKeyPem()
           ?? throw new InvalidOperationException("The certificate carries no exportable RSA or ECDSA private key.");
}

/// <summary>
/// The TLS service each secure port owns on this reactor, keyed by the port a connection arrived
/// on. One per port, since ALPN and the client CA differ per endpoint; filled from
/// <see cref="Server.ResolveTls"/> when the reactor starts, and read per connection by the
/// connection driver.
/// </summary>
internal sealed class TlsRegistry
{
    private readonly Dictionary<ushort, TlsService> _byPort = [];

    public void Add(ushort port, TlsService service) => _byPort[port] = service;

    public bool TryFor(ushort port, out TlsService service) => _byPort.TryGetValue(port, out service!);
}
