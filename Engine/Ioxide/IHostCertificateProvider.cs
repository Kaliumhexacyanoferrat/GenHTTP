using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Engine.Ioxide;

/// <summary>
/// A certificate provider that names the hosts it answers for, so one port can serve a different
/// certificate per name - Server Name Indication.
/// </summary>
/// <remarks>
/// <see cref="Hosts"/> is read once, when the server starts: OpenSSL wants a context per name and
/// ngtcp2 wants the files before it accepts anything, so a provider issuing on demand cannot be
/// served here. The certificate on the binding stays the DEFAULT and answers a client that sent no
/// name, or one not listed, rather than the handshake being refused.
///
/// HTTP/3 needs files: implement <see cref="IFileCertificateProvider"/> too and each host is
/// registered with ngtcp2. A host with only an <c>X509Certificate2</c> serves the TCP transports.
/// </remarks>
public interface IHostCertificateProvider : ICertificateProvider
{
    /// <summary>
    /// The host names this provider answers for, beside the default. Matched case-insensitively;
    /// an international name belongs here in its A-label (<c>xn--</c>) form, which is what a client
    /// actually sends.
    /// </summary>
    IEnumerable<string> Hosts { get; }
}
