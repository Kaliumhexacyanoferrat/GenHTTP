using System.Security.Cryptography.X509Certificates;

namespace GenHTTP.Api.Infrastructure;

/// <summary>
/// Allows secure endpoints to select the certificate they should
/// authenticate the client with.
/// </summary>
public interface ICertificateProvider
{

    /// <summary>
    /// Select a certificate for authentication based on the given host.
    /// </summary>
    /// <param name="host">The name of the host, if specified by the client</param>
    /// <returns>The certificate to be used to authenticate the client</returns>
    X509Certificate2? Provide(string? host);
}
