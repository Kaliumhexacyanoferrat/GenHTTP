using System.Security.Cryptography.X509Certificates;

using GenHTTP.Api.Content.Authentication;

namespace GenHTTP.Modules.Authentication.ClientCertificate;

/// <summary>
/// A simple user implementation that can be used to store
/// the information about a client authenticated with
/// a client certificate.
/// </summary>
public sealed class ClientCertificateUser : IUser
{

    #region Get-/Setters

    /// <inheritdoc />
    public string DisplayName => Certificate.Subject;

    /// <summary>
    /// The certificate the client authenticated with.
    /// </summary>
    public X509Certificate Certificate { get; }

    #endregion

    #region Initialization

    /// <summary>
    /// Creates a new user instance for the given certificate.
    /// </summary>
    /// <param name="clientCertificate">The certificate the client authenticated with</param>
    public ClientCertificateUser(X509Certificate clientCertificate)
    {
        Certificate = clientCertificate;
    }

    #endregion

}
