namespace GenHTTP.Api.Protocol;

/// <summary>
/// The protocol used by the connected client.
/// </summary>
public enum ClientProtocol
{

    /// <summary>
    /// Plain, non encrypted HTTP.
    /// </summary>
    Http,

    /// <summary>
    /// HTTP via TLS.
    /// </summary>
    Https

}
