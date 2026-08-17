using System.Net;
using System.Security.Cryptography.X509Certificates;

using GenHTTP.Api.Infrastructure;
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
/// What each transport accepts, and in what form. Everything is named once, on the binding; the
/// forms differ because OpenSSL takes the certificate as data while ngtcp2 loads it by path:
/// </para>
///
/// <code>
///                          TCP (HTTP/1.1, HTTP/2)          HTTP/3 (QUIC)
///   server certificate     PEM files, or X509Certificate2  PEM files only
///   server key             the same, whichever was given   PEM files only
///   issuer chain           from the file, else rebuilt      from the file
///   client trust anchors   ClientCaPath or ClientCaPem     ClientCaPath or ClientCaPem
///   demand a client cert   ICertificateValidator.RequireCertificate, on both
/// </code>
///
/// <para>
/// A plain <c>ICertificateProvider</c> answers with an <c>X509Certificate2</c> and serves TCP only:
/// HTTP/3 on such a binding is refused when the server is built, rather than started without the
/// listener it was asked for. An <see cref="IFileCertificateProvider"/> names files as well and
/// serves both.
/// </para>
///
/// <para>
/// Files are preferred wherever both are on offer, which also settles the chain. OpenSSL reads a
/// chain file whole, so intermediates come from the file the binding named. Only the in-memory form
/// needs them rebuilt - an <c>X509Certificate2</c> carries no chain, so they are recovered from the
/// machine store and sent leaf-first with the root left off.
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

        // Asked once, here, because both transports settle their certificates when the server
        // starts rather than per handshake: OpenSSL wants a context per name and ngtcp2 wants the
        // files before it accepts anything.
        Files = (security.CertificateProvider as IFileCertificateProvider)?.ProvideFiles(null);

        Hosts = ResolveHosts(security.CertificateProvider);

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
    /// The certificate as files, when the provider has them - what HTTP/3 needs, and what TCP
    /// prefers, since OpenSSL loads a chain file whole. Null when the binding gave only the
    /// in-memory certificate.
    /// </summary>
    public CertificateFiles? Files { get; }

    /// <summary>
    /// The certificates this endpoint serves by name, beside <see cref="Files"/> - empty unless the
    /// binding named an <see cref="IHostCertificateProvider"/>.
    /// </summary>
    public IReadOnlyList<HostCertificate> Hosts { get; }

    /// <summary>
    /// Each name the provider answers for, resolved once. A name is asked for in both forms: the
    /// files, which HTTP/3 needs and TCP prefers, and the object, which is all a plain provider
    /// has. A name that yields neither is dropped here rather than half-registered later.
    /// </summary>
    private static IReadOnlyList<HostCertificate> ResolveHosts(ICertificateProvider provider)
    {
        if (provider is not IHostCertificateProvider byHost)
        {
            return [];
        }

        var files = provider as IFileCertificateProvider;
        var resolved = new List<HostCertificate>();

        foreach (var host in byHost.Hosts)
        {
            if (string.IsNullOrWhiteSpace(host))
            {
                continue;
            }

            var certificate = new HostCertificate(host, files?.ProvideFiles(host), byHost.Provide(host));

            if (certificate.Files is not null || certificate.Certificate is not null)
            {
                resolved.Add(certificate);
            }
        }

        return resolved;
    }

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

/// <summary>
/// One host name and the certificate answering for it, in whichever forms the provider had.
/// </summary>
/// <param name="Host">The name a client asks for, matched case-insensitively.</param>
/// <param name="Files">
/// The PEM paths, when the provider is also an <see cref="IFileCertificateProvider"/>. Required for
/// HTTP/3 - a host without them serves the TCP transports only.
/// </param>
/// <param name="Certificate">The in-memory form, which every provider has.</param>
internal sealed record HostCertificate(string Host, CertificateFiles? Files, X509Certificate2? Certificate);
