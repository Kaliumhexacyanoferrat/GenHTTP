using System.Security.Cryptography.X509Certificates;

namespace GenHTTP.Engine.Ioxide;

/// <summary>One certificate named as PEM files - the form every transport here can serve.</summary>
public sealed class FileCertificateProvider : IFileCertificateProvider
{
    private readonly Lazy<X509Certificate2> _certificate;

    private readonly CertificateFiles _files;

    // Names the PEM files, and reads them only if something asks for the loaded certificate.
    public FileCertificateProvider(string certificatePath, string keyPath)
    {
        _files = new CertificateFiles(certificatePath, keyPath);
        _certificate = new Lazy<X509Certificate2>(() => X509Certificate2.CreateFromPemFile(certificatePath, keyPath));
    }

    // Takes an already loaded certificate alongside the paths HTTP/3 needs.
    public FileCertificateProvider(X509Certificate2 certificate, string certificatePath, string keyPath)
    {
        _files = new CertificateFiles(certificatePath, keyPath);
        _certificate = new Lazy<X509Certificate2>(certificate);
    }

    // The loaded certificate, whatever name was asked for.
    public X509Certificate2 Provide(string? host) => _certificate.Value;

    // The same certificate as paths, which is the only form HTTP/3 takes.
    public CertificateFiles? ProvideFiles(string? host) => _files;
    
}
