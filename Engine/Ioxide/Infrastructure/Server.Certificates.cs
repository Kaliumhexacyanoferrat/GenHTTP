using GenHTTP.Engine.Ioxide.Infrastructure.Endpoints;

using ioxide.tls;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Engine.Ioxide.Infrastructure;

/// <summary>
/// Replacing the certificates of a server that is already serving.
/// </summary>
public sealed partial class Server
{
    private static readonly Dictionary<string, TlsCertificate> NoHosts = [];

    /// <summary>Serialises rotations; the handshake paths take no lock at all.</summary>
    private readonly Lock _reload = new();

    /// <summary>
    /// Asks every bound certificate provider again and installs what it answers with, across both
    /// transports, without dropping a connection - what a renewal hook calls once the ACME client
    /// has rewritten its PEM. Established connections keep the certificate they authenticated with.
    /// </summary>
    /// <remarks>
    /// Only the certificate material changes. Trust anchors, RequireCertificate, ALPN, the TLS floor
    /// and the kTLS pin stay as the binding set them, and no name can be added: both stacks settle
    /// their SNI tables at startup. Everything is resolved and checked before anything is published,
    /// so a provider that throws or a path not yet written leaves the server serving what it had.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// The certificates could not be resolved, and nothing was replaced; or a service refused them,
    /// and the message names what did and did not rotate.
    /// </exception>
    public void ReloadCertificates()
    {
        if (_tlsRegistries is not { } registries || Array.Find(registries, r => r is not null) is not { } started)
        {
            throw new InvalidOperationException(
                "There are no TLS services to rotate: this server either has no secure endpoint or is not running.");
        }

        lock (_reload)
        {
            var rotations = ResolveRotations(started, out var quic);

            if (quic is { } endPoint)
            {
                // First, because it is the transport that publishes atomically: bad material throws
                // here having changed nothing at all.
                ReloadQuicCertificates(endPoint, endPoint.ResolveFiles(), endPoint.ResolveHosts());
            }

            Publish(registries, rotations);

            _logger.LogInformation("Replaced the certificates on {Count} secure port(s)", rotations.Count);
        }
    }

    /// <summary>
    /// What each secure port should serve now. Ports that started without a certificate are
    /// skipped: they have no service to rotate, and only a restart can give them one.
    /// </summary>
    private List<CertificateRotation> ResolveRotations(TlsRegistry started, out SecureEndPoint? quic)
    {
        var rotations = new List<CertificateRotation>();

        quic = null;

        foreach (var endPoint in SecureEndPoints)
        {
            if (ReferenceEquals(endPoint, _quicEndPoint))
            {
                quic = endPoint;
            }

            if (!started.TryFor(endPoint.Port, out _))
            {
                continue;
            }

            var port = endPoint.Port;

            if (ResolveDefaultCertificate(endPoint, endPoint.ResolveFiles(), port) is not { } certificate)
            {
                throw new InvalidOperationException(
                    $"The provider on port {port} no longer answers with a certificate. The port still serves the one it had.");
            }

            var byHost = ResolveHostCertificates(endPoint, endPoint.ResolveHosts(), port);

            RequireFiles(certificate, port, "The default certificate");

            foreach (var (host, hostCertificate) in byHost ?? NoHosts)
            {
                RequireFiles(hostCertificate, port, $"The certificate for {host}");
            }

            rotations.Add(new CertificateRotation(port, certificate, byHost));
        }

        return rotations;
    }

    /// <summary>
    /// Installs the resolved sets on every reactor, collecting failures rather than stopping half
    /// way. A service that refuses the new material keeps the old, which is still a certificate the
    /// endpoint was bound with - so a mixed server serves, it just serves two vintages.
    /// </summary>
    private void Publish(TlsRegistry?[] registries, List<CertificateRotation> rotations)
    {
        List<Exception>? failures = null;

        for (var reactor = 0; reactor < registries.Length; reactor++)
        {
            if (registries[reactor] is not { } registry)
            {
                continue;
            }

            foreach (var rotation in rotations)
            {
                if (!registry.TryFor(rotation.Port, out var service))
                {
                    continue;
                }

                try
                {
                    service.ReplaceCertificates(rotation.Default, rotation.ByHost);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Reactor {Reactor} kept the previous certificate on port {Port}", reactor, rotation.Port);

                    (failures ??= []).Add(e);
                }
            }
        }

        if (failures is not null)
        {
            throw new InvalidOperationException(
                $"{failures.Count} of the server's TLS services refused the new certificates and still serve the previous ones; "
                + "the rest were replaced. See the inner exceptions, and the errors logged against each reactor and port.",
                new AggregateException(failures));
        }
    }

    /// <summary>
    /// Refuses a certificate naming files that are not there - the half-written renewal, which is
    /// worth catching before a service is part way through rotating.
    /// </summary>
    private static void RequireFiles(TlsCertificate certificate, ushort port, string what)
    {
        if (certificate.CertificatePath is { } path && !File.Exists(path))
        {
            throw new InvalidOperationException($"{what} on port {port} names a certificate file that does not exist: {path}");
        }

        if (certificate.KeyPath is { } key && !File.Exists(key))
        {
            throw new InvalidOperationException($"{what} on port {port} names a key file that does not exist: {key}");
        }
    }

    /// <summary>One port's new certificates: the default, and the alternatives by name.</summary>
    private sealed record CertificateRotation(ushort Port, TlsCertificate Default, IReadOnlyDictionary<string, TlsCertificate>? ByHost);
}
