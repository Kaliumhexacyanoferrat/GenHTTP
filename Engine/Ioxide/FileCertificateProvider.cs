using System.Security.Cryptography.X509Certificates;

namespace GenHTTP.Engine.Ioxide;

public sealed class FileCertificateProvider : IFileCertificateProvider
{
    private readonly Lazy<X509Certificate2> _certificate;

    private readonly CertificateFiles _files;

    public FileCertificateProvider(string certificatePath, string keyPath)
    {
        _files = new CertificateFiles(certificatePath, keyPath);
        _certificate = new Lazy<X509Certificate2>(() => X509Certificate2.CreateFromPemFile(certificatePath, keyPath));
    }

    public FileCertificateProvider(X509Certificate2 certificate, string certificatePath, string keyPath)
    {
        _files = new CertificateFiles(certificatePath, keyPath);
        _certificate = new Lazy<X509Certificate2>(certificate);
    }

    public X509Certificate2 Provide(string? host) => _certificate.Value;

    public CertificateFiles? ProvideFiles(string? host) => _files;
}
