using System.Net;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Ioxide;

using ioxide;

using GenHTTP.Modules.Files;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

using GenHTTP.Playground;

using IoxideServer = GenHTTP.Engine.Ioxide.Infrastructure.Server;

// The namespace and the class share a name, so the class needs an alias to be reachable.
using IoxideFilesModule = GenHTTP.Modules.IoxideFiles.IoxideFiles;

// One host, a protocol combination per port. HTTP/1.1 and HTTP/2 share a TCP socket, HTTP/3 is a
// UDP socket on the same port number, and any combination of the three is allowed.
//
//   http://localhost:8080    HTTP/1.1
//   http://localhost:8081    HTTP/2 only (h2c) - an HTTP/1.1 client is turned away
//   http://localhost:8082    both, on one socket: the connection preface decides
//   https://localhost:8443   HTTP/1.1 + HTTP/2 over TCP and HTTP/3 over QUIC, with a certificate
//                            per host name (SNI) for alpha.localhost and beta.localhost
//   https://localhost:8444   HTTP/1.1 behind mutual TLS - a client certificate is required, and
//                            one signed by the wrong CA is refused
//
//     dotnet run -c Release --project Playground
//
//     curl http://localhost:8080/ok
//     curl --http2-prior-knowledge http://localhost:8081/ok
//     curl -k --http1.1    https://localhost:8443/ok
//     curl -k --http2      https://localhost:8443/ok
//     curl -k --http3-only https://localhost:8443/ok
//     curl -kv --resolve alpha.localhost:8443:127.0.0.1 https://alpha.localhost:8443/ok
//     curl -k --cert certs/client.crt   --key certs/client.key   https://localhost:8444/ok
//     curl -k --cert certs/impostor.crt --key certs/impostor.key https://localhost:8444/ok
//
//     kill -HUP <pid>   mints new certificates and installs them on the running server
//
// The sample writes its own throwaway PEM into ./certs - a server certificate per name, plus a
// client CA, a client it signs and an impostor it does not. A deployment binds the files its ACME
// client already writes instead. ngtcp2 loads PEM by path, so HTTP/3 always needs files; the TCP
// transports take either those or an X509Certificate2.
//
// Where two protocols share the TCP socket, ALPN decides on a secure port and the HTTP/2 connection
// preface decides on a plaintext one. Only one endpoint may serve HTTP/3: the server binds a single
// QUIC listener, and a second is refused at startup. Browsers never try HTTP/3 first - they connect
// over TCP and move to QUIC once an Alt-Svc header points them at it.
//
// Two static handlers over the SAME directory, so the difference can be priced rather than argued:
// /ring/* is IoxideFiles (descriptors shared across reactors, read positionally off the ring,
// nothing cached in memory) and /disk/* is GenHTTP's built-in Files module. GENHTTP_STATIC picks
// the directory; without it neither route is mounted.
//
//     GENHTTP_STATIC=/srv/www dotnet run -c Release --project Playground
//     wrk -t8 -c64 -d8s http://127.0.0.1:8080/ring/asset.bin

var staticDir = Environment.GetEnvironmentVariable("GENHTTP_STATIC");

var app = Layout.Create()
                .Add("ok", Content.From(Resource.FromString("ok")));

if (staticDir != null && Directory.Exists(staticDir))
{
    app = app.Add("ring", IoxideFilesModule.From(staticDir))
             .Add("disk", Assets.From(staticDir));
}

string[] hosts = ["localhost", "alpha.localhost", "beta.localhost"];

var (defaultCert, defaultKey) = Certs.Server(hosts[0]);

// One provider, three certificates: the default answers a client that sent no name or an unknown
// one, and the two names get their own. Named as files, so HTTP/3 serves all three as well.
var certificates = new HostCertificateProvider(defaultCert, defaultKey);

foreach (var name in hosts[1..])
{
    var (certificate, key) = Certs.Server(name);

    certificates.Add(name, certificate, key);
}

var clientCa = Certs.ClientPki();

var server = Host.Create(
                   options: new EngineOptions
                   {
                       Protocols = Protocols.Http1,

                       ProtocolsByPort =
                       {
                           [8081] = Protocols.Http2,
                           [8082] = Protocols.Http1AndHttp2,
                           [8443] = Protocols.All,
                           [8444] = Protocols.Http1,
                       },

                       Reactor = new ReactorOptions
                       {
                           ReactorCount = 2,
                           RingEntries = 8192,
                           RecvBufferSize = 32 * 1024,
                           RecvSlots = 4096,
                           Incremental = null,
                       },

                       Tcp = new TcpTransportOptions
                       {
                           HandshakeTimeoutMs = 10_000,
                           CipherSuites = null,
                           CipherList = null,
                           TxKernelTls = false,
                           RxKernelTls = false,
                           ListenBacklog = 1024,
                           WriteSlabSize = 16 * 1024,
                           WriteOverflow = WriteOverflowStrategy.Grow,
                           PoolMax = 1024,
                           ZeroCopySend = false,
                           RecvQueueEntries = 64,
                       },

                       Http3 = new Http3Options
                       {
                           HandshakeTimeoutMs = 10_000,
                           IdleTimeoutMs = 60_000,
                           Routing = QuicRouting.Forward,
                           PinMigratedPeers = true,
                           SocketBufferBytes = 8 * 1024 * 1024,
                           QpackDynamicTableCapacity = 4096,
                           QpackBlockedStreams = 100,
                       },
                   })
               .Handler(app)
               .Bind(IPAddress.Loopback, 8080)
               .Bind(IPAddress.Loopback, 8081)
               .Bind(IPAddress.Loopback, 8082)
               .Bind(IPAddress.Loopback, 8443, certificates)
               .Bind(IPAddress.Loopback, 8444, new FileCertificateProvider(defaultCert, defaultKey),
                     certificateValidator: new RequireClientCertificate(clientCa));

await server.StartAsync();

// Renewal, as an ACME hook would do it: rewrite the PEM the providers already name, then ask the
// server to install it. Connections in flight keep the certificate they authenticated with.
using var renew = PosixSignalRegistration.Create(PosixSignal.SIGHUP, context =>
{
    context.Cancel = true;

    foreach (var name in hosts)
    {
        Certs.Server(name);
    }

    (server.Instance as IoxideServer)?.ReloadCertificates();
});

Console.WriteLine($"kill -HUP {Environment.ProcessId} rotates the certificates");

var stopping = new TaskCompletionSource();

Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    stopping.TrySetResult();
};

AppDomain.CurrentDomain.ProcessExit += (_, _) => stopping.TrySetResult();

await stopping.Task;
await server.StopAsync();

/// <summary>
/// Marks an endpoint as requiring a client certificate, and names the CA the offered chain is
/// validated against. Both travel with the endpoint, so another binding can require none or trust a
/// different issuer.
/// </summary>
/// <remarks>
/// OpenSSL (or ngtcp2 on HTTP/3) validates the chain against that CA, so a bad one is refused
/// before a request exists and <see cref="Validate"/> is never called.
/// </remarks>
internal sealed class RequireClientCertificate(string clientCaPath) : IMutualTlsValidator
{
    public bool RequireCertificate => true;

    public string? ClientCaPath => clientCaPath;

    public X509RevocationMode RevocationCheck => X509RevocationMode.NoCheck;

    //TODO: Proper client cert validation example
    public bool Validate(X509Certificate? certificate, X509Chain? chain, SslPolicyErrors policyErrors) => true;
}
