using GenHTTP.Engine.Ioxide.Infrastructure.Endpoints;

using ioxide.tls;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Engine.Ioxide.Infrastructure;

public sealed partial class Server
{
    private static readonly Dictionary<string, TlsCertificate> NoHosts = [];

    private readonly Lock _reload = new();

    public void ReloadCertificates()
    {
        if (!Running || !SecureEndPoints.Any())
        {
            throw new InvalidOperationException(
                "There are no certificates to rotate: this server either has no secure endpoint or is not running.");
        }

        lock (_reload)
        {
            TcpTlsRegistry?[]? registries = _tcpTlsRegistries;

            TcpTlsRegistry? started = registries is null ? null : Array.Find(registries, static r => r is not null);

            var rotations = ResolveRotations(started, out var quic);

            if (quic is { } endPoint)
            {
                // First, because it is the transport that publishes atomically: bad material
                // throws here having changed nothing at all.
                ReloadQuicCertificates(endPoint, endPoint.ResolveFiles(), endPoint.ResolveHosts());
            }

            if (registries is not null)
            {
                Publish(registries, rotations);
            }

            _logger.LogInformation("Replaced the certificates on {TcpPorts} TCP port(s) and {QuicListeners} QUIC listener(s)",
                rotations.Count, quic is not null ? 1 : 0);
        }
    }

    private List<CertificateRotation> ResolveRotations(TcpTlsRegistry? started, out SecureEndPoint? quic)
    {
        var rotations = new List<CertificateRotation>();

        quic = null;

        foreach (var endPoint in SecureEndPoints)
        {
            if (ReferenceEquals(endPoint, _quicEndPoint))
            {
                quic = endPoint;
            }

            if (started is null || !started.TryFor(endPoint.Port, out _))
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

    private void Publish(TcpTlsRegistry?[] registries, List<CertificateRotation> rotations)
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

    private sealed record CertificateRotation(ushort Port, TlsCertificate Default, IReadOnlyDictionary<string, TlsCertificate>? ByHost);
}
