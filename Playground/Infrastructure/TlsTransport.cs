using System.IO.Pipelines;

using GenHTTP.Engine.Ioxide;

using ioxide;
using ioxide.tls;

namespace genhttp.Infrastructure;

/// <summary>
/// TLS configuration for the json-tls profile: a second listener on :8081 with kTLS TX offload,
/// read from TLS_CERT / TLS_KEY (defaulting to the harness-mounted /certs). The transport plumbing
/// lives in the engine (<see cref="IoxideTls"/> / <c>TlsDuplexPipe</c>); this just supplies the
/// cert/key and wires the per-reactor service + the connection factory's port decision.
/// </summary>
public static class Tls
{
    public const ushort Port = 8081;

    public static string? CertPath { get; private set; }

    public static string? KeyPath { get; private set; }

    public static bool Enabled => CertPath is not null;

    public static bool Configure()
    {
        var cert = Environment.GetEnvironmentVariable("TLS_CERT") ?? "/certs/server.crt";
        var key = Environment.GetEnvironmentVariable("TLS_KEY") ?? "/certs/server.key";

        if (File.Exists(cert) && File.Exists(key))
        {
            CertPath = cert;
            KeyPath = key;
            return true;
        }

        return false;
    }

    // onReactorStart: register the ring-native TLS service (OpenSSL ctx) on this reactor.
    public static void StartService(Reactor reactor)
        => IoxideTls.StartService(reactor, new TlsOptions { CertificatePath = CertPath!, KeyPath = KeyPath! });

    // connectionFactory: TLS-terminate the :8081 listener; the main (:8080) port stays plaintext.
    public static ValueTask<IDuplexPipe> CreatePipe(Connection conn)
        => conn.ListenerPort == Port
            ? IoxideTls.AcceptAsync(conn)
            : new ValueTask<IDuplexPipe>(new ConnectionDualPipe(conn));
}
