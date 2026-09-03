using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;

using GenHTTP.Engine.Ioxide.Infrastructure.Endpoints;

using ioxide;
using ioxide.tls;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Engine.Ioxide.Infrastructure;

public sealed partial class Server
{
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

    private void StartTlsServices(Reactor reactor, int index, IReadOnlyList<KeyValuePair<ushort, TlsOptions>> portsTlsOptions)
    {
        TcpTlsRegistry tcpTlsRegistry = new TcpTlsRegistry();

        foreach ((ushort port, TlsOptions options) in portsTlsOptions)
        {
            tcpTlsRegistry.Add(port, TlsService.Start(reactor, options, register: false));
        }

        reactor.AddService(tcpTlsRegistry);

        _tcpTlsRegistries![index] = tcpTlsRegistry;
    }

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

    private TlsProtocolVersion ResolveMinProtocolVersion(SecureEndPoint endPoint)
    {
        var protocols = endPoint.SecurityConfiguration.Protocols;

        var tls12 = protocols.HasFlag(SslProtocols.Tls12);
        var tls13 = protocols.HasFlag(SslProtocols.Tls13);

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

    private bool MutualTlsConfigured => SecureEndPoints.Any(e => e.MutualTls);

    private IEnumerable<SecureEndPoint> SecureEndPoints => _endPoints.OfType<SecureEndPoint>();

    private IEnumerable<SecureEndPoint> SecureTcpEndPoints
        => SecureEndPoints.Where(e => (e.Protocols & Protocols.Http1AndHttp2) != 0);

    private string ExportChainPem(X509Certificate2 certificate, ushort port)
    {
        using var chain = new X509Chain();

        chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
        chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
        chain.ChainPolicy.DisableCertificateDownloads = true;

        // Built for its elements, not its verdict: a privately issued certificate does not
        // validate against this machine's roots and still carries the chain to send.
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

    private static bool IsSelfIssued(X509Certificate2 certificate)
        => certificate.SubjectName.RawData.AsSpan().SequenceEqual(certificate.IssuerName.RawData);

    private static string ExportKeyPem(X509Certificate2 certificate)
        => certificate.GetRSAPrivateKey()?.ExportPkcs8PrivateKeyPem()
           ?? certificate.GetECDsaPrivateKey()?.ExportPkcs8PrivateKeyPem()
           ?? throw new InvalidOperationException("The certificate carries no exportable RSA or ECDSA private key.");
}

internal sealed class TcpTlsRegistry
{
    private readonly Dictionary<ushort, TlsService> _byPort = [];

    public void Add(ushort port, TlsService service) => _byPort[port] = service;

    public bool TryFor(ushort port, out TlsService service) => _byPort.TryGetValue(port, out service!);
}
