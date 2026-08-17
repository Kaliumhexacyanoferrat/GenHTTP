using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Engine.Ioxide;

/// <summary>
/// A certificate provider that can also name its certificate and key as files. Pass one to
/// <c>Bind</c> and the endpoint can serve HTTP/3.
/// </summary>
/// <remarks>
/// <see cref="ICertificateProvider"/> answers with an <c>X509Certificate2</c>, which suits a
/// transport that terminates TLS in managed code. ngtcp2 is a C library that loads PEM by path and
/// takes nothing else, so an endpoint serving HTTP/3 has to be able to name files - and the engine
/// will not manufacture them by writing a private key out on your behalf, since a key left in a
/// temporary directory outlives any shutdown that skips cleanup.
///
/// Files are preferred wherever both forms are available: OpenSSL loads a chain file whole, so the
/// intermediates come from the file rather than being recovered from the machine store.
/// </remarks>
public interface IFileCertificateProvider : ICertificateProvider
{
    /// <summary>
    /// The certificate and key as PEM paths, or null if this provider has only the in-memory form -
    /// in which case the endpoint can still serve HTTP/1.1 and HTTP/2, but not HTTP/3.
    /// </summary>
    /// <param name="host">The host, if the client named one. QUIC asks once, with null.</param>
    CertificateFiles? ProvideFiles(string? host);
}

/// <summary>
/// A certificate chain and its private key, as paths to PEM files.
/// </summary>
/// <param name="Certificate">
/// The certificate, leaf first. Any intermediates in the same file are sent with it, which is how
/// a client reaches a root it trusts - a file holding the leaf alone serves only clients that
/// already have the rest.
/// </param>
/// <param name="Key">The private key, PEM, matching the certificate.</param>
public sealed record CertificateFiles(string Certificate, string Key);
