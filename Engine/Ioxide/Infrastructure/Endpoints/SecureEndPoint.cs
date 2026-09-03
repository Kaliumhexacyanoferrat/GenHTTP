using System.Net;
using System.Security.Cryptography.X509Certificates;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Engine.Shared.Infrastructure;

namespace GenHTTP.Engine.Ioxide.Infrastructure.Endpoints;

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

    public SecurityConfiguration SecurityConfiguration { get; }

    public CertificateFiles? CertificateFiles { get; }

    public IReadOnlyList<HostCertificate> Hosts { get; }

    public CertificateFiles? ResolveFiles()
        => (SecurityConfiguration.CertificateProvider as IFileCertificateProvider)?.ProvideFiles(null);

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

internal sealed record HostCertificate(string Host, CertificateFiles? Files, X509Certificate2? Certificate);
