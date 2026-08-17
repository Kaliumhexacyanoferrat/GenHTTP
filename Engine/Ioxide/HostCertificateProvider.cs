using System.Security.Cryptography.X509Certificates;

namespace GenHTTP.Engine.Ioxide;

/// <summary>
/// Serves a different certificate per host name, and one default for everyone else - one port,
/// several sites.
/// </summary>
/// <remarks>
/// The ready-made <see cref="IHostCertificateProvider"/>, so a deployment with a certificate per
/// site on disk does not have to write one. Everything is named as files, which is what lets the
/// same binding serve HTTP/1.1, HTTP/2 and HTTP/3 for every name.
///
/// <code>
/// var certificates = new HostCertificateProvider("/certs/default.crt", "/certs/default.key")
///     .Add("alpha.example", "/certs/alpha.crt", "/certs/alpha.key")
///     .Add("beta.example",  "/certs/beta.crt",  "/certs/beta.key");
///
/// host.Bind(IPAddress.Any, 443, certificates);
/// </code>
///
/// The default answers a client that sends no name - anything connecting by address - and any name
/// not added here, rather than the handshake being refused. Names are matched case-insensitively;
/// an international name belongs here in its A-label (<c>xn--</c>) form.
/// </remarks>
public sealed class HostCertificateProvider : IHostCertificateProvider, IFileCertificateProvider
{
    private readonly Dictionary<string, FileCertificateProvider> _byHost =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly FileCertificateProvider _default;

    /// <param name="certificatePath">The default certificate, leaf first, with any intermediates.</param>
    /// <param name="keyPath">The private key, PEM, matching that certificate.</param>
    public HostCertificateProvider(string certificatePath, string keyPath)
    {
        _default = new FileCertificateProvider(certificatePath, keyPath);
    }

    /// <summary>
    /// Serves <paramref name="certificatePath"/> to clients asking for <paramref name="host"/>.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// The name is blank, or already added - which would otherwise leave a certificate in the table
    /// that could never be served, since the first one registered is the one that answers.
    /// </exception>
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

    /// <summary>
    /// The provider for this name, or the default. An unknown name falling back rather than
    /// failing is what keeps the port reachable by address.
    /// </summary>
    private FileCertificateProvider Resolve(string? host)
        => host is not null && _byHost.TryGetValue(host, out var provider) ? provider : _default;
}
