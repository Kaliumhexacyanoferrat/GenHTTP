using System.Security.Cryptography.X509Certificates;

namespace GenHTTP.Api.Infrastructure.Certificates;

public class ObjectCertificateProvider(X509Certificate2 certificate) : ICertificateProvider
{

    public X509Certificate2? Provide(string? host) => certificate;
    
}
