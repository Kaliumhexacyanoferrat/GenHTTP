using System.Security.Cryptography.X509Certificates;

namespace GenHTTP.Engine.Ioxide;

/// <summary>
/// Serves one certificate in both forms this engine's transports take: the PEM files ngtcp2 loads
/// for HTTP/3, and an <see cref="X509Certificate2"/> for anything wanting the object.
/// </summary>
public sealed class FileCertificateProvider : IFileCertificateProvider
{
    private readonly Lazy<X509Certificate2> _certificate;

    private readonly CertificateFiles _files;

    /// <summary>
    /// From files alone. The object form is loaded only if something asks for it, so on this
    /// engine - which prefers the files - the private key stays out of managed memory.
    /// </summary>
    /// <param name="certificatePath">The certificate, leaf first, with any intermediates.</param>
    /// <param name="keyPath">The private key, PEM, matching that certificate.</param>
    public FileCertificateProvider(string certificatePath, string keyPath)
    {
        _files = new CertificateFiles(certificatePath, keyPath);
        _certificate = new Lazy<X509Certificate2>(() => X509Certificate2.CreateFromPemFile(certificatePath, keyPath));
    }

    /// <summary>
    /// From a certificate already in hand and the files holding it. The two are not checked against
    /// each other.
    /// </summary>
    public FileCertificateProvider(X509Certificate2 certificate, string certificatePath, string keyPath)
    {
        _files = new CertificateFiles(certificatePath, keyPath);
        _certificate = new Lazy<X509Certificate2>(certificate);
    }

    public X509Certificate2 Provide(string? host) => _certificate.Value;

    public CertificateFiles? ProvideFiles(string? host) => _files;
}
