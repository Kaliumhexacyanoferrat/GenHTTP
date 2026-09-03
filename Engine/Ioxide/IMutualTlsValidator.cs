using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Engine.Ioxide;

/// <summary>
/// A client-certificate validator that also names what the offered chain is validated against.
/// Pass one to <c>Bind</c> and that endpoint does mutual TLS against these anchors.
/// </summary>
/// <remarks>
/// ioxide validates in OpenSSL, and in ngtcp2 for HTTP/3, both of which need the anchors before the
/// handshake - early enough that a bad chain is refused before any request exists. So the anchors
/// travel with the validator rather than being handed over per connection.
///
/// <c>Validate</c> still runs afterwards on the TCP transports, against the peer certificate
/// OpenSSL settled on, for whatever the application wants to decide for itself. It does not run
/// over HTTP/3: ngtcp2 exposes the peer's subject and common name but no certificate, so there is
/// nothing to hand it there.
/// </remarks>
public interface IMutualTlsValidator : ICertificateValidator
{
    /// <summary>
    /// PEM bundle of trust anchors, as a path. Its subject names are also sent in the
    /// CertificateRequest, so a client holding several certificates can pick the right one;
    /// <see cref="ClientCaPem"/> sends no such hint.
    /// </summary>
    string? ClientCaPath => null;

    /// <summary>The trust anchors as PEM text - the in-memory alternative to <see cref="ClientCaPath"/>.</summary>
    string? ClientCaPem => null;
}
