using System.Net;

using GenHTTP.Engine.Shared.Infrastructure;

namespace GenHTTP.Engine.Ioxide.Infrastructure.Endpoints;

/// <summary>
/// An endpoint bound with a certificate: what TLS termination here takes, in one place. The
/// handshake itself belongs to the transport - OpenSSL for HTTP/1.1 and HTTP/2, ngtcp2 for HTTP/3 -
/// so this carries the settings rather than performing it.
/// </summary>
/// <remarks>
/// The trust anchors come from <c>EngineOptions</c> - the engine-wide ones, or the set named for
/// this port - since GenHTTP's <c>Bind</c> carries no bundle of its own. Whether a client
/// certificate is demanded is settled here: that setting and the binding's own validator are ORed
/// at construction rather than at each use.
/// </remarks>
internal sealed class SecureEndPoint : EndPoint
{
    internal SecureEndPoint(IPAddress? address, ushort port, bool dualStack, Protocols protocols,
        SecurityConfiguration security, MutualTlsOptions mutualTls)
        : base(address, port, dualStack, protocols)
    {
        Security = security;

        ClientCaPath = mutualTls.ClientCaPath;
        ClientCaPem = mutualTls.ClientCaPem;

        RequireClientCertificate = mutualTls.RequireClientCertificate
                                   || security.CertificateValidator?.RequireCertificate == true;
    }

    public override bool Secure => true;

    /// <summary>How this endpoint is secured: its certificate provider, protocols and validator.</summary>
    public SecurityConfiguration Security { get; }

    /// <summary>
    /// PEM bundle of trust anchors that client certificates are validated against, as a path. Its
    /// subject names are also sent in the CertificateRequest, so a client holding several
    /// certificates can pick the right one; <see cref="ClientCaPem"/> sends no such hint.
    /// </summary>
    public string? ClientCaPath { get; }

    /// <summary>The trust anchors as PEM text - the in-memory alternative to <see cref="ClientCaPath"/>.</summary>
    public string? ClientCaPem { get; }

    /// <summary>
    /// Whether a client offering no certificate is refused here. False still asks for one and
    /// validates what arrives, since the CertificateRequest goes out either way.
    /// </summary>
    public bool RequireClientCertificate { get; }

    /// <summary>Whether this endpoint asks for a client certificate at all.</summary>
    public bool MutualTls => RequireClientCertificate
                             || ClientCaPath is not null
                             || ClientCaPem is not null
                             || Security.CertificateValidator is not null;
}
