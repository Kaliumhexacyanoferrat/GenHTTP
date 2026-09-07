using System.Security.Cryptography.X509Certificates;

using GenHTTP.Api.Infrastructure.Certificates;

namespace GenHTTP.Api.Infrastructure;

/// <summary>
/// Helper class to create a certificate provider that can be used to
/// bind a hosting endpoint.
/// </summary>
public static class CertificateProvider
{

    /// <summary>
    /// Creates a certificate provider from the given certificate.
    /// </summary>
    /// <param name="certificate">The certificate to be used to secure the endpoint</param>
    /// <returns>The newly created certificate provider</returns>
    public static ICertificateProvider From(X509Certificate2 certificate) => new ObjectCertificateProvider(certificate);

}
