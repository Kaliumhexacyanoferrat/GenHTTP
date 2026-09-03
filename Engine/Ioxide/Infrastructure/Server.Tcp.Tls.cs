using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;

using GenHTTP.Engine.Ioxide.Infrastructure.Endpoints;

using ioxide;
using ioxide.tls;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Engine.Ioxide.Infrastructure;

/// <summary>
/// TLS termination for the TCP endpoints - HTTP/1.1 and HTTP/2 both ride this.
/// </summary>
public sealed partial class Server
{
    /// <summary>
    /// The TLS options for every secure port that can produce a certificate. A port that cannot
    /// stays advertised as secure - so upgrade redirects still work - but refuses its handshakes.
    /// </summary>
    private IEnumerable<KeyValuePair<ushort, TlsOptions>> ResolveTls()
    {
        foreach (var endPoint in SecureTcpEndPoints)
        {
            var port = endPoint.Port;

            if (ResolveDefaultCertificate(endPoint, endPoint.CertificateFiles, port) is not { } certificate)
            {
                _logger.LogWarning("No default certificate for secure port {Port}; handshakes there will be refused.", port);
                continue;
            }

            yield return new KeyValuePair<ushort, TlsOptions>(port, new TlsOptions
            {
                CertificatePath = certificate.CertificatePath,
                KeyPath = certificate.KeyPath,
                CertificatePem = certificate.CertificatePem,
                KeyPem = certificate.KeyPem,

                CertificatesByHost = ResolveHostCertificates(endPoint, endPoint.Hosts, port),

                // Server preference, most preferred first.
                Alpn = endPoint.Protocols.HasFlag(Protocols.Http2) ? ["h2", "http/1.1"] : ["http/1.1"],

                ClientCaPath = endPoint.ClientCaPath,
                ClientCaPem = endPoint.ClientCaPem,
                RequireClientCertificate = endPoint.RequireClientCertificate,

                MinProtocolVersion = ResolveMinProtocolVersion(endPoint),

                HandshakeTimeoutMs = _engineOptions.Tcp.HandshakeTimeoutMs,
                CipherSuites = _engineOptions.Tcp.CipherSuites,
                CipherList = _engineOptions.Tcp.CipherList,

                KernelTx = _engineOptions.Tcp.TxKernelTls,
                KernelRx = _engineOptions.Tcp.RxKernelTls
            });
        }
    }

    /// <summary>
    /// Starts one TLS service per secure TCP port on this reactor, and publishes the registry that
    /// finds them. Runs on the reactor's own thread, from <c>OnStart</c>.
    /// </summary>
    /// <remarks>
    /// A service per port rather than one for the server: ALPN, the client CA and the TLS floor are
    /// all per endpoint. They are started unregistered because <c>Reactor.GetService</c> is
    /// last-write-wins by type, so the registry is what gets registered and the port picks the
    /// service out of it.
    /// </remarks>
    private void StartTlsServices(Reactor reactor, int index, IReadOnlyList<KeyValuePair<ushort, TlsOptions>> portsTlsOptions)
    {
        TcpTlsRegistry tcpTlsRegistry = new TcpTlsRegistry();

        foreach ((ushort port, TlsOptions options) in portsTlsOptions)
        {
            tcpTlsRegistry.Add(port, TlsService.Start(reactor, options, register: false));
        }

        reactor.AddService(tcpTlsRegistry);

        // Also kept on the server, so ReloadCertificates can reach every reactor's services.
        _tcpTlsRegistries![index] = tcpTlsRegistry;
    }

    /// <summary>
    /// The certificate answering a client that asked for no name, or an unknown one. CertificateFiles are
    /// preferred: OpenSSL reads a chain file whole, so intermediates come from the file and the key
    /// never enters managed memory. Null where the provider has neither form.
    /// </summary>
    private TlsCertificate? ResolveDefaultCertificate(SecureEndPoint endPoint, CertificateFiles? certificateFiles, ushort port)
    {
        if (certificateFiles is not null)
        {
            return new TlsCertificate { CertificatePath = certificateFiles.Certificate, KeyPath = certificateFiles.Key };
        }

        if (endPoint.SecurityConfiguration.CertificateProvider.Provide(null) is { } certificate)
        {
            return new TlsCertificate
            {
                CertificatePem = ExportChainPem(certificate, port),
                KeyPem = ExportKeyPem(certificate),
            };
        }

        return null;
    }

    /// <summary>
    /// The certificates this port serves by name. Null where the binding named no hosts, which
    /// leaves the handshake without a servername callback at all.
    /// </summary>
    /// <remarks>
    /// Client verification is not repeated per host and does not need to be: OpenSSL fixes the
    /// verify mode from the default context, so a name cannot select its way out of mutual TLS.
    /// </remarks>
    private IReadOnlyDictionary<string, TlsCertificate>? ResolveHostCertificates(SecureEndPoint endPoint,
        IReadOnlyList<HostCertificate> hosts, ushort port)
    {
        if (hosts.Count == 0)
        {
            return null;
        }

        var byHost = new Dictionary<string, TlsCertificate>(hosts.Count, StringComparer.OrdinalIgnoreCase);

        foreach (var host in hosts)
        {
            if (byHost.ContainsKey(host.Host))
            {
                _logger.LogWarning("Port {Port} names the host {Host} more than once; only the first certificate can ever be served, so the rest were dropped.", port, host.Host);
                continue;
            }

            byHost[host.Host] = host.Files is { } files
                ? new TlsCertificate { CertificatePath = files.Certificate, KeyPath = files.Key }
                : new TlsCertificate
                {
                    CertificatePem = ExportChainPem(host.Certificate!, port),
                    KeyPem = ExportKeyPem(host.Certificate!),
                };
        }

        return byHost.Count > 0 ? byHost : null;
    }

    /// <summary>
    /// The TLS floor for one endpoint, from the <c>SslProtocols</c> its binding named.
    /// </summary>
    /// <remarks>
    /// <c>SslProtocols</c> is a set; OpenSSL takes a minimum and has no maximum. So a set is
    /// honoured exactly when it is contiguous and open at the top - <c>Tls13</c> and
    /// <c>Tls12 | Tls13</c> are, anything below 1.2 and 1.2-without-1.3 are not. Those two warn and
    /// widen rather than throw: a defensible default elsewhere should not be a dead deployment
    /// here, and serving more than was asked for is what an operator needs told.
    /// </remarks>
    private TlsProtocolVersion ResolveMinProtocolVersion(SecureEndPoint endPoint)
    {
        var protocols = endPoint.SecurityConfiguration.Protocols;

        var tls12 = protocols.HasFlag(SslProtocols.Tls12);
        var tls13 = protocols.HasFlag(SslProtocols.Tls13);

        // Named without naming them, so no obsolete member has to be suppressed.
        if ((protocols & ~(SslProtocols.Tls12 | SslProtocols.Tls13)) != 0)
        {
            _logger.LogWarning(
                "Port {Port} was bound asking for a TLS version below 1.2. This engine terminates TLS in OpenSSL, whose floor is 1.2, "
                + "so those versions are not served and never were; the endpoint serves {Served}.",
                endPoint.Port, tls13 && !tls12 ? "TLS 1.3" : "TLS 1.2 and above");
        }

        if (tls12 && !tls13)
        {
            _logger.LogWarning(
                "Port {Port} was bound for TLS 1.2 without 1.3. ioxide takes a minimum version and has no maximum, so 1.3 stays "
                + "available on this endpoint - it is negotiated only when the client prefers it, and it is the stronger of the two.",
                endPoint.Port);
        }

        return (tls12, tls13) switch
        {
            (false, true) => TlsProtocolVersion.Tls13,
            (true, _) => TlsProtocolVersion.Tls12,
            _ => TlsProtocolVersion.Default,
        };
    }

    /// <summary>
    /// Says once, at startup, which parts of a bound <c>ICertificateValidator</c> this engine
    /// cannot honour - rather than leaving it to be inferred from a client that was let in.
    /// </summary>
    private void WarnAboutValidatorGaps()
    {
        foreach (var endPoint in SecureEndPoints)
        {
            if (endPoint.SecurityConfiguration.CertificateValidator is not { } validator)
            {
                continue;
            }

            if (validator.RevocationCheck != X509RevocationMode.NoCheck)
            {
                _logger.LogWarning(
                    "The validator on port {Port} asks for {Mode} revocation checking, which this engine does not perform - neither OpenSSL "
                    + "nor ngtcp2 is given a CRL or an OCSP responder here, so a revoked client certificate is accepted until it expires. "
                    + "Use short-lived certificates, or check the peer in ICertificateValidator.Validate against your own source of truth.",
                    endPoint.Port, validator.RevocationCheck);
            }

            if (endPoint.Protocols.HasFlag(Protocols.Http3))
            {
                _logger.LogWarning(
                    "Port {Port} serves HTTP/3 with a certificate validator. Validate runs on the TCP transports, where OpenSSL hands over the "
                    + "peer certificate, but NOT over QUIC: ngtcp2 exposes only the peer's subject and common name, so there is no certificate "
                    + "to hand it. An HTTP/3 client is admitted on the chain check and RequireCertificate alone. Keep the two transports on "
                    + "separate ports if that difference matters.",
                    endPoint.Port);
            }
        }
    }

    /// <summary>Whether any endpoint asks for a client certificate at all.</summary>
    private bool MutualTlsConfigured => SecureEndPoints.Any(e => e.MutualTls);

    /// <summary>The endpoints bound with a certificate - the ones TLS applies to.</summary>
    private IEnumerable<SecureEndPoint> SecureEndPoints => _endPoints.OfType<SecureEndPoint>();

    /// <summary>
    /// The secure endpoints a TLS service is actually worth building for: the ones that accept TCP.
    /// </summary>
    /// <remarks>
    /// An HTTP/3-only port is secure and has no TCP listener - <c>ResolveTcpPorts</c> leaves it out
    /// and <c>BuildServerConfig</c> then binds none - so a context built for it on every reactor
    /// could never be reached. QUIC terminates its own TLS in ngtcp2, from the certificate paths the
    /// binding named. A port serving HTTP/3 alongside HTTP/1.1 or HTTP/2 still belongs here.
    /// </remarks>
    private IEnumerable<SecureEndPoint> SecureTcpEndPoints
        => SecureEndPoints.Where(e => (e.Protocols & Protocols.Http1AndHttp2) != 0);

    /// <summary>
    /// The certificate and the intermediates a client needs to reach a root it trusts, leaf first
    /// and root omitted (RFC 8446 4.4.2). Only the in-memory form needs this - an
    /// <c>X509Certificate2</c> carries no chain, so it is rebuilt from the machine store. AIA
    /// downloads stay off: a slow issuer host would hang startup rather than slow it.
    /// </summary>
    private string ExportChainPem(X509Certificate2 certificate, ushort port)
    {
        using var chain = new X509Chain();

        chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
        chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
        chain.ChainPolicy.DisableCertificateDownloads = true;

        // Built for its elements, not its verdict: a privately issued certificate does not validate
        // against this machine's roots and still carries the chain to send.
        chain.Build(certificate);

        var links = chain.ChainElements;

        if (links.Count <= 1)
        {
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
/// The TLS service each secure TCP port owns on this reactor, keyed by the port a connection
/// arrived on. One per port, since ALPN and the client CA differ per endpoint.
/// </summary>
/// <remarks>
/// TCP only, as the name says: QUIC terminates TLS in ngtcp2 through its own engine, so an
/// HTTP/3-only port holds nothing here. The registry itself is still registered whenever any
/// endpoint is secure - empty if none of them serve TCP - because <c>ReloadCertificates</c> reaches
/// every reactor through it, including to rotate the QUIC certificate.
/// </remarks>
internal sealed class TcpTlsRegistry
{
    private readonly Dictionary<ushort, TlsService> _byPort = [];

    public void Add(ushort port, TlsService service) => _byPort[port] = service;

    public bool TryFor(ushort port, out TlsService service) => _byPort.TryGetValue(port, out service!);
}
