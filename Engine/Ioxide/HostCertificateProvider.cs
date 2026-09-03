using System.Security.Cryptography.X509Certificates;

namespace GenHTTP.Engine.Ioxide;

public sealed class HostCertificateProvider : IHostCertificateProvider, IFileCertificateProvider
{
    private readonly Dictionary<string, FileCertificateProvider> _byHost =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly FileCertificateProvider _default;

    public HostCertificateProvider(string certificatePath, string keyPath)
    {
        _default = new FileCertificateProvider(certificatePath, keyPath);
    }

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

    public X509Certificate2 Provide(string? host) => Resolve(host).Provide(host);

    public CertificateFiles? ProvideFiles(string? host) => Resolve(host).ProvideFiles(host);

    private FileCertificateProvider Resolve(string? host)
        => host is not null && _byHost.TryGetValue(host, out var provider) ? provider : _default;
}
