using System.Net;
using System.Security.Cryptography.X509Certificates;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Infrastructure;

/// <summary>
/// The remote client which requests a resource from the server.
/// </summary>
public interface IClientConnection
{

    /// <summary>
    /// The IP address of the remotely connected client.
    /// </summary>
    IPAddress IPAddress { get; }

    /// <summary>
    /// The protocol used by the client to connect
    /// to the server.
    /// </summary>
    ClientProtocol? Protocol { get; }

    /// <summary>
    /// The host name used by the client to connect
    /// to the server.
    /// </summary>
    string? Host { get; }

    /// <summary>
    /// The certificate used by the client to connect.
    /// </summary>
    X509Certificate? Certificate { get; }

}
