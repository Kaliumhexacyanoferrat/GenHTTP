using GenHTTP.Modules.Authentication.ClientCertificate;

namespace GenHTTP.Modules.Authentication;

public static class ClientCertificateAuthentication
{

    /// <summary>
    /// Creates a concern that checks access permission based on the
    /// certificate presented by the client during the SSL handshake.
    /// </summary>
    /// <returns>The newly created concern</returns>
    public static ClientCertificateAuthenticationBuilder Create() => new();

}
