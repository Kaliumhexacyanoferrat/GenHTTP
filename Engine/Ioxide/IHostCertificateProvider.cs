using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Engine.Ioxide;

/// <summary>
/// A certificate provider that can say which host names it answers for, so one port can serve a
/// different certificate per name - Server Name Indication.
/// </summary>
/// <remarks>
/// <see cref="ICertificateProvider.Provide"/> already takes the host the client asked for, but a
/// callback alone is not enough here: both transports settle their certificates when the server
/// starts, not per handshake. OpenSSL wants a context per name and ngtcp2 wants the files
/// registered before it accepts anything, so the engine has to know the names up front - which is
/// what this adds.
///
/// The consequence worth knowing: <see cref="Hosts"/> is read once, at startup, and each name is
/// asked for once. A provider that wants to answer a name it has never seen - issuing on demand,
/// say - cannot be served by this engine; bind the names you intend to answer for.
///
/// The certificate on the binding stays the DEFAULT. A client that sends no name, or asks for one
/// not in <see cref="Hosts"/>, is answered with it rather than refused - so the port is still
/// reachable by address, and a client that dislikes the certificate says so itself.
///
/// HTTP/3 needs files, as ever: implement <see cref="IFileCertificateProvider"/> as well and each
/// host is registered with ngtcp2 too. A host that has only an <c>X509Certificate2</c> serves
/// HTTP/1.1 and HTTP/2 for its name and is left out of the QUIC listener, which is the same rule
/// the default certificate already follows.
/// </remarks>
public interface IHostCertificateProvider : ICertificateProvider
{
    /// <summary>
    /// The host names this provider answers for, beside the default. Read once, when the server
    /// starts. Matched case-insensitively; an international name belongs here in its A-label
    /// (<c>xn--</c>) form, which is what a client actually sends.
    /// </summary>
    IEnumerable<string> Hosts { get; }
}
