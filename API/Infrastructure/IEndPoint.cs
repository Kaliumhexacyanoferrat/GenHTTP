using System.Net;

namespace GenHTTP.Api.Infrastructure;

/// <summary>
///     An endpoint the server will listen on for incoming requests.
/// </summary>
public interface IEndPoint : IDisposable
{

    /// <summary>
    ///     The IP address the endpoint is bound to.
    /// </summary>
    /// <remarks>
    ///     Can be a specific IPv4/IPv6 address or a more generic one
    ///     such as <see cref="System.Net.IPAddress.Any" />.
    /// </remarks>
    IPAddress IPAddress { get; }

    /// <summary>
    ///     The port the endpoint is listening on.
    /// </summary>
    ushort Port { get; }

    /// <summary>
    ///     Specifies, whether this is is an endpoint secured via SSL/TLS.
    /// </summary>
    bool Secure { get; }
}
