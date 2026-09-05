using System.Security.Cryptography.X509Certificates;

namespace GenHTTP.Engine.Ioxide;

/// <summary>A certificate per host name, picked by the name the client asked for.</summary>
public sealed class HostCertificateProvider : IHostCertificateProvider, IFileCertificateProvider
{
    private readonly Dictionary<string, FileCertificateProvider> _byHost =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly FileCertificateProvider _default;

    // Sets the certificate for clients that name no host, or one nothing here answers for.
    public HostCertificateProvider(string certificatePath, string keyPath)
    {
        _default = new FileCertificateProvider(certificatePath, keyPath);
    }

    // Adds the certificate for one name, refusing a name that already has one.
    public HostCertificateProvider Add(string host, string certificatePath, string keyPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(host);

        if (!_byHost.TryAdd(host, new FileCertificateProvider(certificatePath, keyPath)))
        {
            throw new ArgumentException($"A certificate for '{host}' was already added.", nameof(host));
        }

        return this;
    }

    public IEnumerable<string> Hosts => _byHost.Keys;

    // The certificate for this name, or the default.
    public X509Certificate2 Provide(string? host) => Resolve(host).Provide(host);

    // The paths for this name, or the default's.
    public CertificateFiles? ProvideFiles(string? host) => Resolve(host).ProvideFiles(host);

    // Matches a name case-insensitively, falling back to the default.
    private FileCertificateProvider Resolve(string? host)
        => host is not null && _byHost.TryGetValue(host, out var provider) ? provider : _default;
}
