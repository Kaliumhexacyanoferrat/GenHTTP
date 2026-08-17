using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Engine.Ioxide;

/// <summary>
/// A client-certificate validator that also names what the offered chain is validated against.
/// Pass one to <c>Bind</c> and that endpoint does mutual TLS against these anchors.
/// </summary>
/// <remarks>
/// <see cref="ICertificateValidator"/> is called with a chain that has already been built, which
/// suits an engine validating in managed code. ioxide validates in OpenSSL, and in ngtcp2 for
/// HTTP/3, both of which need the trust anchors before the handshake begins - early enough that a
/// bad chain is refused before any request exists, and <c>Validate</c> is never reached. So the
/// anchors travel with the validator, on the binding that wanted them.
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
