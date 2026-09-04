using System.Net;
using System.Security.Cryptography.X509Certificates;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Engine.Shared.Infrastructure;

namespace GenHTTP.Engine.Ioxide.Infrastructure.Endpoints;

/// <summary>An endpoint bound with a certificate, carrying everything its TLS was configured with.</summary>
internal sealed class SecureEndPoint : EndPoint
{
    // Reads the binding's TLS settings once, since both stacks settle their tables at startup.
    internal SecureEndPoint(IPAddress? address, ushort port, bool dualStack, HttpProtocols httpProtocols,
        SecurityConfiguration securityConfiguration)
        : base(address, port, dualStack, httpProtocols)
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

    public SecurityConfiguration SecurityConfiguration { get; }

    public CertificateFiles? CertificateFiles { get; }

    public IReadOnlyList<HostCertificate> Hosts { get; }

    // Asks the provider for the default certificate as paths, now rather than at startup.
    public CertificateFiles? ResolveFiles()
        => (SecurityConfiguration.CertificateProvider as IFileCertificateProvider)?.ProvideFiles(null);

    // Asks the provider for every name it answers for, in both forms, skipping the ones it has neither for.
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

    public string? ClientCaPath { get; }

    public string? ClientCaPem { get; }

    public bool RequireClientCertificate { get; }

    public bool MutualTls => SecurityConfiguration.CertificateValidator is not null;
}

/// <summary>One name a provider answers for, in both forms - files for HTTP/3, in-memory for TCP.</summary>
internal sealed record HostCertificate(string Host, CertificateFiles? Files, X509Certificate2? Certificate);
