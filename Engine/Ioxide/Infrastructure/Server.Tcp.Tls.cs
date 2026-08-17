using System.Security.Cryptography.X509Certificates;
using System.Text;

using GenHTTP.Engine.Ioxide.Infrastructure.Endpoints;

using ioxide.tls;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Engine.Ioxide.Infrastructure;

/// <summary>
/// TLS termination for the TCP endpoints - HTTP/1.1 and HTTP/2 both ride this.
/// </summary>
public sealed partial class Server
{
    /// <summary>
    /// The TLS options for every secure port the binding can produce a certificate for. A provider
    /// with neither files nor a default (no-SNI) certificate leaves the port advertised as secure -
    /// so secure-upgrade redirects still work - but refusing its handshakes.
    /// </summary>
    /// <remarks>
    /// Files are preferred where the provider has them. OpenSSL reads a chain file whole, so the
    /// intermediates come from the file the binding named rather than being recovered from the
    /// machine store, and the private key never enters managed memory.
    /// </remarks>
    private IEnumerable<KeyValuePair<ushort, TlsOptions>> ResolveTls()
    {
        foreach (var endPoint in SecureEndPoints)
        {
            var port = endPoint.Port;

            // ioxide takes exactly one source, so only one pair of these is ever set.
            string? certificatePath = null, keyPath = null, certificatePem = null, keyPem = null;

            if (endPoint.Files is { } files)
            {
                certificatePath = files.Certificate;
                keyPath = files.Key;
            }
            else if (endPoint.Security.CertificateProvider.Provide(null) is { } certificate)
            {
                certificatePem = ExportChainPem(certificate, port);
                keyPem = ExportKeyPem(certificate);
            }
            else
            {
                _logger.LogWarning("No default certificate for secure port {Port}; handshakes there will be refused (SNI selection is unsupported).", port);
                continue;
            }

            yield return new(port, new TlsOptions
            {
                CertificatePath = certificatePath,
                KeyPath = keyPath,
                CertificatePem = certificatePem,
                KeyPem = keyPem,

                // Server preference, most preferred first. A client offering neither continues
                // without an ALPN extension at all.
                Alpn = endPoint.Protocols.HasFlag(Protocols.Http2) ? ["h2", "http/1.1"] : ["http/1.1"],

                ClientCaPath = endPoint.ClientCaPath,
                ClientCaPem = endPoint.ClientCaPem,
                RequireClientCertificate = endPoint.RequireClientCertificate,

                KernelTx = _engineOptions.Tcp.TxKernelTls,
                KernelRx = _engineOptions.Tcp.RxKernelTls
            });
        }
    }

    /// <summary>
    /// Whether any endpoint asks for a client certificate at all.
    /// </summary>
    private bool MutualTlsConfigured => SecureEndPoints.Any(e => e.MutualTls);

    /// <summary>The endpoints bound with a certificate - the ones TLS applies to.</summary>
    private IEnumerable<SecureEndPoint> SecureEndPoints => _endPoints.OfType<SecureEndPoint>();

    /// <summary>
    /// The certificate and the intermediates a client needs to reach a root it trusts, leaf first.
    /// </summary>
    /// <remarks>
    /// <c>ICertificateProvider</c> hands over one certificate, but anything issued by a real
    /// CA is signed by an intermediate, and a client that does not already hold that intermediate
    /// cannot build a path to its root - so a server sends them (RFC 8446 4.4.2). The Internal
    /// engine gets this for free from <c>SslStream</c>, which assembles the chain itself; this
    /// engine terminates TLS on its own, so it assembles it here.
    ///
    /// The root is left out deliberately: a client that does not already trust it will not start
    /// because we sent it, and it is bytes on every handshake.
    ///
    /// Certificate downloads are off. Fetching a missing intermediate over AIA would put a network
    /// call on the startup path, where a slow or unreachable host is a hung server rather than a
    /// slow one - the intermediate is expected in the machine store beside the certificate.
    /// </remarks>
    private string ExportChainPem(X509Certificate2 certificate, ushort port)
    {
        using var chain = new X509Chain();

        chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
        chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
        chain.ChainPolicy.DisableCertificateDownloads = true;

        // Built for its elements, not its verdict: a privately issued or self-signed certificate
        // does not validate against this machine's roots and still carries the chain to send.
        chain.Build(certificate);

        var links = chain.ChainElements;

        if (links.Count <= 1)
        {
            // Self-signed is the ordinary case here - leaf and root at once, nothing to add. Any
            // other certificate arriving alone means its issuer was not found, and the handshake
            // will fail for clients that cannot supply the gap themselves.
            if (!IsSelfIssued(certificate))
            {
                _logger.LogWarning(
                    "No issuer chain found for the certificate on port {Port} ({Subject}), so only the leaf will be sent. "
                    + "Clients without its intermediates cached will refuse the handshake; install them in the machine store.",
                    port, certificate.Subject);
            }

            return certificate.ExportCertificatePem();
        }

        var pem = new StringBuilder();

        for (var i = 0; i < links.Count; i++)
        {
            var link = links[i].Certificate;

            if (i == links.Count - 1 && IsSelfIssued(link))
            {
                break;
            }

            pem.AppendLine(link.ExportCertificatePem());
        }

        return pem.ToString();
    }

    /// <summary>Whether a certificate is its own issuer, which is what makes it a root.</summary>
    private static bool IsSelfIssued(X509Certificate2 certificate)
        => certificate.SubjectName.RawData.AsSpan().SequenceEqual(certificate.IssuerName.RawData);

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
