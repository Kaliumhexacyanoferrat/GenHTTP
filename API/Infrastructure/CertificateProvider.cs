using System.Security.Cryptography.X509Certificates;

using GenHTTP.Api.Infrastructure.Certificates;

namespace GenHTTP.Api.Infrastructure;

public static class CertificateProvider
{

    public static ICertificateProvider From(X509Certificate2 certificate) => new ObjectCertificateProvider(certificate);

}
