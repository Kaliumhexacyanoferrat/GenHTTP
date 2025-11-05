namespace GenHTTP.Modules.Security;

/// <summary>
/// Specifies the strategy of the server to redirect
/// unsecure requests to HTTPS secured endpoints.
/// </summary>
public enum SecureUpgrade
{

    /// <summary>
    /// The server will not attempt to upgrade requests.
    /// </summary>
    None,

    /// <summary>
    /// Clients may request an upgrade via the Upgrade-Insecure-Requests
    /// header. Aside from that, the server will not attempt to upgrade
    /// insecure requests.
    /// </summary>
    Allow,

    /// <summary>
    /// The server will always redirect requests to an insecure endpoint
    /// to the HTTPS secured one.
    /// </summary>
    Force

}
