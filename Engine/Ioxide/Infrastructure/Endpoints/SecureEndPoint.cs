using System.Net;

using GenHTTP.Engine.Shared.Infrastructure;

namespace GenHTTP.Engine.Ioxide.Infrastructure.Endpoints;

/// <summary>
/// An endpoint bound with a certificate: what TLS termination here takes, in one place. The
/// handshake itself belongs to the transport - OpenSSL for HTTP/1.1 and HTTP/2, ngtcp2 for HTTP/3 -
/// so this carries the settings rather than performing it.
/// </summary>
/// <remarks>
/// Everything here comes off the binding: the certificate from <c>Bind</c>, and the mutual-TLS
/// settings from the validator passed alongside it. A validator asks for a client certificate; an
/// <see cref="IMutualTlsValidator"/> also names what the chain is validated against. Read once
/// here rather than at each use, so the transports ask the endpoint and nothing else.
///
/// <para>
/// What each transport accepts, and in what form. The two do not match, because OpenSSL is handed
/// the certificate as data while ngtcp2 loads it by path:
/// </para>
///
/// <code>
///                          TCP (HTTP/1.1, HTTP/2)          HTTP/3 (QUIC)
///   server certificate     X509Certificate2, from Bind     PEM file, Http3.CertificatePath
///   server key             exported from that certificate  PEM file, Http3.KeyPath
///   issuer chain           built here, root omitted        whatever that PEM file holds
///   client trust anchors   ClientCaPath or ClientCaPem     ClientCaPath or ClientCaPem
///   demand a client cert   ICertificateValidator.RequireCertificate, on both
/// </code>
///
/// <para>
/// Three consequences of that table. The HTTP/3 certificate is named a SECOND time, on
/// <c>EngineOptions.Http3</c>, because ngtcp2 takes paths and the engine will not write a private
/// key out on anyone's behalf - it should be the same certificate the endpoint is bound with, and
/// the QUIC half warns when the thumbprints disagree.
/// </para>
///
/// <para>
/// The issuer chain is assembled for TCP only. <c>ICertificateProvider</c> yields a single
/// certificate, which cannot carry intermediates, so they are recovered from the machine store and
/// sent leaf-first with the root left off; HTTP/3 sends whatever the configured PEM file contains.
/// </para>
///
/// <para>
/// Client anchors are the one setting that reads the same on both, in either form. That took
/// ioxide 0.5.192: before it, ngtcp2 took only a path, so <see cref="ClientCaPem"/> reached OpenSSL
/// and was dropped on the way to QUIC - an endpoint serving both transports validated clients over
/// TCP and let them through unvalidated over HTTP/3, saying nothing about it.
/// </para>
/// </remarks>
internal sealed class SecureEndPoint : EndPoint
{
    internal SecureEndPoint(IPAddress? address, ushort port, bool dualStack, Protocols protocols,
        SecurityConfiguration security)
        : base(address, port, dualStack, protocols)
    {
        Security = security;

        RequireClientCertificate = security.CertificateValidator?.RequireCertificate == true;

        if (security.CertificateValidator is IMutualTlsValidator mutualTls)
        {
            ClientCaPath = mutualTls.ClientCaPath;
            ClientCaPem = mutualTls.ClientCaPem;
        }
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

    /// <summary>
    /// Whether this endpoint asks for a client certificate at all - which is exactly whether the
    /// binding named a validator, since everything mutual TLS needs now arrives on one.
    /// </summary>
    public bool MutualTls => Security.CertificateValidator is not null;
}
