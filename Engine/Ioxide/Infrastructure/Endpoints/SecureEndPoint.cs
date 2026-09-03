using System.Net;
using System.Security.Cryptography.X509Certificates;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Engine.Shared.Infrastructure;

namespace GenHTTP.Engine.Ioxide.Infrastructure.Endpoints;

/// <summary>
/// An endpoint bound with a certificate: what TLS termination takes, in one place. The handshake
/// belongs to the transport - OpenSSL for HTTP/1.1 and HTTP/2, ngtcp2 for HTTP/3.
/// </summary>
/// <remarks>
/// <code>
///                          TCP (HTTP/1.1, HTTP/2)          HTTP/3 (QUIC)
///   server certificate     PEM files, or X509Certificate2  PEM files only
///   issuer chain           from the file, else rebuilt     from the file
///   client trust anchors   ClientCaPath or ClientCaPem     ClientCaPath or ClientCaPem
///   demand a client cert   ICertificateValidator.RequireCertificate, on both
/// </code>
/// CertificateFiles are preferred wherever both are offered, which also settles the chain: OpenSSL reads a
/// chain file whole, while an <c>X509Certificate2</c> carries no chain and needs it rebuilt from
/// the machine store. A plain <c>ICertificateProvider</c> therefore serves TCP only.
/// </remarks>
internal sealed class SecureEndPoint : EndPoint
{
    internal SecureEndPoint(IPAddress? address, ushort port, bool dualStack, Protocols protocols,
        SecurityConfiguration securityConfiguration)
        : base(address, port, dualStack, protocols)
    {
        SecurityConfiguration = securityConfiguration;

        CertificateFiles = ResolveFiles();
        Hosts = ResolveHosts();

        RequireClientCertificate = securityConfiguration.CertificateValidator?.RequireCertificate == true;

        if (securityConfiguration.CertificateValidator is IMutualTlsValidator mutualTls)
        {
            ClientCaPath = mutualTls.ClientCaPath;
            ClientCaPem = mutualTls.ClientCaPem;
        }
    }

    public override bool Secure => true;

    /// <summary>How this endpoint is secured: its certificate provider, protocols and validator.</summary>
    public SecurityConfiguration SecurityConfiguration { get; }

    /// <summary>
    /// The certificate as files, as it was when the server started. Null where the binding gave
    /// only the in-memory form. A reload asks <see cref="ResolveFiles"/> again.
    /// </summary>
    public CertificateFiles? CertificateFiles { get; }

    /// <summary>
    /// The certificates served by name, as they were when the server started. Empty unless the
    /// binding named an <see cref="IHostCertificateProvider"/>.
    /// </summary>
    public IReadOnlyList<HostCertificate> Hosts { get; }

    /// <summary>The default certificate as files, asked of the provider now.</summary>
    public CertificateFiles? ResolveFiles()
        => (SecurityConfiguration.CertificateProvider as IFileCertificateProvider)?.ProvideFiles(null);

    /// <summary>
    /// Each name the provider answers for, asked of it now, in both forms - the files HTTP/3 needs
    /// and the object a plain provider has. A name yielding neither is dropped here rather than
    /// half-registered later. Only names present at startup can be served: both stacks settle their
    /// tables then, so a reload replaces the certificates behind the names, not the set of them.
    /// </summary>
    public IReadOnlyList<HostCertificate> ResolveHosts()
    {
        if (SecurityConfiguration.CertificateProvider is not IHostCertificateProvider byHost)
        {
            return [];
        }

        var files = SecurityConfiguration.CertificateProvider as IFileCertificateProvider;
        var resolved = new List<HostCertificate>();

        foreach (var host in byHost.Hosts)
        {
            if (string.IsNullOrWhiteSpace(host))
            {
                continue;
            }

            var certificate = new HostCertificate(host, files?.ProvideFiles(host), byHost.Provide(host));

            if (certificate.Files is not null || certificate.Certificate is not null)
            {
                resolved.Add(certificate);
            }
        }

        return resolved;
    }

    /// <summary>
    /// PEM bundle of trust anchors client certificates are validated against, as a path. Its
    /// subject names are sent in the CertificateRequest, so a client holding several can pick;
    /// <see cref="ClientCaPem"/> sends no such hint.
    /// </summary>
    public string? ClientCaPath { get; }

    /// <summary>The trust anchors as PEM text - the in-memory alternative to <see cref="ClientCaPath"/>.</summary>
    public string? ClientCaPem { get; }

    /// <summary>
    /// Whether a client offering no certificate is refused. False still asks for one and validates
    /// what arrives, since the CertificateRequest goes out either way.
    /// </summary>
    public bool RequireClientCertificate { get; }

    /// <summary>Whether this endpoint asks for a client certificate at all.</summary>
    public bool MutualTls => SecurityConfiguration.CertificateValidator is not null;
}

/// <summary>
/// One host name and the certificate answering for it, in whichever forms the provider had.
/// <paramref name="Files"/> is required for HTTP/3; a name without them serves TCP only.
/// </summary>
internal sealed record HostCertificate(string Host, CertificateFiles? Files, X509Certificate2? Certificate);
