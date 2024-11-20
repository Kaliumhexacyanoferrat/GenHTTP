using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace GenHTTP.Api.Infrastructure;

/// <summary>
/// When passed to the binding method of the server builder, implementations
/// of this interface will be called to negotiate client authentication
/// during the SSL handshake.
/// </summary>
public interface ICertificateValidator
{

    /// <summary>
    /// If true, the client is expected to send a certificate for
    /// authentication. If no certificates are sent, the handshake
    /// will fail and no connection can be established.
    /// </summary>
    bool RequireCertificate => true;

    /// <summary>
    /// Specifies how the server should check whether the client
    /// certificate has been revoked using a CRL.
    /// </summary>
    X509RevocationMode RevocationCheck => X509RevocationMode.Offline;

    /// <summary>
    /// Will be called for each certificate presented by the client
    /// to validate, whether this certificate should be use for the
    /// connection.
    /// </summary>
    /// <param name="certificate">The certificate presented by the client</param>
    /// <param name="chain">The chain of certificate authorities associated with the remote certificate</param>
    /// <param name="policyErrors">One or more errors associated with the remote certificate</param>
    /// <returns>A boolean value that determines whether the specified certificate is accepted for authentication</returns>
    bool Validate(X509Certificate? certificate, X509Chain? chain, SslPolicyErrors policyErrors);

}
