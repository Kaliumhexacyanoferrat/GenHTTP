using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Engine.Ioxide;

/// <summary>
/// A certificate provider that can also name its certificate and key as files. Pass one to
/// <c>Bind</c> and the endpoint can serve HTTP/3.
/// </summary>
/// <remarks>
/// ngtcp2 loads PEM by path and takes nothing else, and the engine will not write a private key out
/// on your behalf. Files are preferred on the TCP transports too: OpenSSL loads a chain file whole,
/// so the intermediates come from the file rather than the machine store.
/// </remarks>
public interface IFileCertificateProvider : ICertificateProvider
{
    /// <summary>
    /// The certificate and key as PEM paths, or null where this provider has only the in-memory
    /// form - which serves HTTP/1.1 and HTTP/2, but not HTTP/3.
    /// </summary>
    /// <param name="host">The host, if the client named one. QUIC asks once, with null.</param>
    CertificateFiles? ProvideFiles(string? host);
}

/// <summary>
/// A certificate chain and its private key, as paths to PEM files.
/// </summary>
/// <param name="Certificate">
/// The certificate, leaf first. Intermediates in the same file are sent with it; a file holding the
/// leaf alone serves only clients that already have the rest.
/// </param>
/// <param name="Key">The private key, PEM, matching the certificate.</param>
public sealed record CertificateFiles(string Certificate, string Key);
