using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Ioxide;

using GenHTTP.Modules.Files;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

// The namespace and the class share a name, so the class needs an alias to be reachable.
using IoxideFilesModule = GenHTTP.Modules.IoxideFiles.IoxideFiles;

// One host, a protocol combination per port. HTTP/1.1 and HTTP/2 share a TCP socket, HTTP/3 is a
// UDP socket on the same port number, and any combination of the three is allowed.
//
//   http://localhost:8080     Http1           HTTP/1.1 only
//   http://localhost:8081     Http2           HTTP/2 only (h2c) - an HTTP/1.1 client is turned away
//   http://localhost:8082     Http1AndHttp2   both on one socket, the preface decides
//   https://localhost:8443    All             HTTP/1.1 + HTTP/2 over TCP, HTTP/3 over UDP
//   https://localhost:8444    Http1           HTTP/1.1 behind mutual TLS - a client certificate is
//                                             required, and one signed by the wrong CA is refused
//
//     dotnet run -c Release --project Playground
//
//     curl http://localhost:8080/ok
//     curl --http2-prior-knowledge http://localhost:8081/ok
//     curl --http1.1 http://localhost:8082/ok
//     curl -k --http1.1 https://localhost:8443/ok
//     curl -k --http2 https://localhost:8443/ok
//     curl -k --http3-only https://localhost:8443/ok
//     curl -k --cert certs/client.crt --key certs/client.key https://localhost:8444/ok
//
// The mutual TLS port needs a client certificate to answer at all, so the sample writes a CA, a
// certificate signed by it and a second signed by nobody into ./certs on startup, and prints the
// commands. Validation happens in OpenSSL against the CA - the endpoint's certificateValidator is
// what marks the port as requiring one.
//
// Where two protocols share the TCP socket, ALPN decides during the handshake on a secure port and
// the HTTP/2 connection preface decides on a plaintext one. HTTP/3 always needs a certificate,
// since QUIC carries TLS 1.3 and has no cleartext mode.
//
// Http1AndHttp3 and Http2AndHttp3 exist too - HTTP/3 alongside just one of the TCP protocols - and
// so does Http3 on its own, which opens a UDP socket and no TCP listener at all. Only one of them
// can be live here: a server binds a single QUIC listener, so a second endpoint asking for HTTP/3
// is refused at startup. Change what 8443 serves to try another.
//
// Browsers never try HTTP/3 first. They connect over TCP and only move to QUIC once a response has
// told them where to look, so a browser-facing deployment adds an Alt-Svc header pointing at the
// HTTP/3 port. curl reaches it directly with --http3-only, which is why the sample needs nothing.
//
// Two static handlers over the SAME directory, so the difference can be priced rather than argued:
//
//   /ring/*  IoxideFiles - ioxide.file opens every file once, shares the descriptors across
//            reactors and reads them positionally off the io_uring ring. Nothing is cached in
//            memory, so resident size stays flat whatever the asset set weighs.
//   /disk/*  GenHTTP's built-in Files module, for comparison.
//
// GENHTTP_STATIC picks the directory; without it neither route is mounted.
//
//     GENHTTP_STATIC=/srv/www dotnet run -c Release --project Playground
//     wrk -t8 -c64 -d8s http://127.0.0.1:8080/ring/asset.bin
//     wrk -t8 -c64 -d8s http://127.0.0.1:8080/disk/asset.bin

// One reactor per core is the default. A sample does not need the whole machine, and this is not
// where throughput is measured - bench/ is.
const int Reactors = 2;

var staticDir = Environment.GetEnvironmentVariable("GENHTTP_STATIC");

var app = Layout.Create()
                .Add("ok", Content.From(Resource.FromString("ok")));

if (staticDir != null && Directory.Exists(staticDir))
{
    app = app.Add("ring", IoxideFilesModule.From(staticDir))
             .Add("disk", Assets.From(staticDir));
}

// A throwaway certificate so the sample runs with no setup. Point GENHTTP_CERT at a PKCS#12 bundle
// to serve a real one - a browser will refuse HTTP/3 to a certificate it does not trust.
using var certificate = LoadCertificate();

// ngtcp2 loads PEM by path, so serving HTTP/3 means having the certificate on disk. The engine will
// not write one out on your behalf, so the sample writes its own throwaway certificate here and
// names it below. A deployment points at the PEM it already has.
var (serverCertPath, serverKeyPath) = WriteServerCertificate(certificate);

// A CA, a client it signs, and an impostor it does not - so the mutual TLS port below can be tried
// both ways without generating anything by hand.
var clientCa = WriteClientCertificates();

await Host.Create(
              configure: c => c with { ReactorCount = Reactors },
              options: new IoxideOptions
              {
                  // What a port serves unless named below.
                  Protocols = IoxideProtocols.Http1,

                  ProtocolsByPort =
                  {
                      [8081] = IoxideProtocols.Http2,
                      [8082] = IoxideProtocols.Http1AndHttp2,
                      [8443] = IoxideProtocols.All,
                      [8444] = IoxideProtocols.Http1,
                  },

                  MutualTls = new IoxideMutualTlsOptions
                  {
                      // What an offered client certificate is validated against. WHICH ports ask
                      // for one is decided per endpoint, by the validator passed to Bind - so 8443
                      // stays open while 8444 requires a certificate.
                      ClientCaPath = clientCa,
                  },

                  Http3 = new IoxideHttp3Options
                  {
                      // Bytes of QPACK dynamic table offered to HTTP/3 clients. 0 keeps every
                      // header literal, which costs bytes but can never stall a stream on a table
                      // update. In practice only browsers advertise a table of their own.
                      QpackDynamicTableCapacity = 4096,
                      QpackBlockedStreams = 100,

                      // The SAME certificate passed to Bind below, named again because ngtcp2
                      // loads PEM by path - OpenSSL, which terminates HTTP/1.1 and HTTP/2, takes
                      // the PEM text directly and touches no disk. Required for HTTP/3: the engine
                      // refuses to start a QUIC listener without it rather than writing a key out.
                      //
                      CertificatePath = serverCertPath,
                      KeyPath = serverKeyPath,
                  },
              })
          .Handler(app)
          .Bind(IPAddress.Loopback, 8080)
          .Bind(IPAddress.Loopback, 8081)
          .Bind(IPAddress.Loopback, 8082)
          .Bind(IPAddress.Loopback, 8443, certificate)
          // mTLS
          .Bind(IPAddress.Loopback, 8444, certificate, certificateValidator: new RequireClientCertificate())
          .RunAsync();

/// <summary>
/// Writes the server certificate as PEM, so ngtcp2 can load it by path.
/// </summary>
static (string Certificate, string Key) WriteServerCertificate(X509Certificate2 certificate)
{
    var directory = Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "certs"));

    var certPath = Path.Combine(directory.FullName, "server.crt");
    var keyPath = Path.Combine(directory.FullName, "server.key");

    File.WriteAllText(certPath, certificate.ExportCertificatePem());

    WritePrivateKey(keyPath, certificate.GetRSAPrivateKey()?.ExportPkcs8PrivateKeyPem()
                             ?? throw new InvalidOperationException("The development certificate carries no RSA private key."));

    return (certPath, keyPath);
}

/// <summary>
/// Writes a client CA, a certificate it signs, and one it does not, and returns the CA's path.
/// </summary>
/// <remarks>
/// A mutual TLS endpoint cannot be tried without a client certificate, so the sample makes one
/// rather than asking you to. The impostor exists so the refusal can be seen as well as the pass.
/// </remarks>
static string WriteClientCertificates()
{
    var directory = Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "certs"));

    // One instant for every certificate here. Reading the clock per certificate gives the CA and
    // the client it signs windows a second apart, and a leaf outliving its issuer is refused.
    var from = DateTimeOffset.UtcNow.AddDays(-1);
    var until = from.AddYears(1);

    using var caKey = RSA.Create(2048);

    var caRequest = new CertificateRequest("CN=playground client CA", caKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    caRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));

    using var ca = caRequest.CreateSelfSigned(from, until);

    var caPath = Path.Combine(directory.FullName, "client-ca.crt");
    File.WriteAllText(caPath, ca.ExportCertificatePem());

    Issue(ca, "client", "CN=alice", directory.FullName, from, until);
    Issue(null, "impostor", "CN=mallory", directory.FullName, from, until);

    return caPath;

    static void Issue(X509Certificate2? issuer, string name, string subject, string directory,
        DateTimeOffset from, DateTimeOffset until)
    {
        using var key = RSA.Create(2048);

        var request = new CertificateRequest(subject, key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        // Signed by the CA the server trusts, or self-signed - which is the impostor.
        using var certificate = issuer is null
            ? request.CreateSelfSigned(from, until)
            : request.Create(issuer, from, until, Guid.NewGuid().ToByteArray());

        File.WriteAllText(Path.Combine(directory, $"{name}.crt"), certificate.ExportCertificatePem());
        WritePrivateKey(Path.Combine(directory, $"{name}.key"), key.ExportPkcs8PrivateKeyPem());
    }
}

static X509Certificate2 LoadCertificate()
{
    if (Environment.GetEnvironmentVariable("GENHTTP_CERT") is { Length: > 0 } path && File.Exists(path))
    {
        return X509CertificateLoader.LoadPkcs12FromFile(path, Environment.GetEnvironmentVariable("GENHTTP_CERT_PASSWORD"));
    }

    using var key = RSA.Create(2048);

    var request = new CertificateRequest("CN=localhost", key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

    // Without a subject alternative name nothing modern will verify this, only skip the check.
    var names = new SubjectAlternativeNameBuilder();
    names.AddDnsName("localhost");
    names.AddIpAddress(IPAddress.Loopback);
    request.CertificateExtensions.Add(names.Build());

    using var generated = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1));

    // The private key has to come back through a PKCS#12 round trip before a TLS stack will use it.
    return X509CertificateLoader.LoadPkcs12(generated.Export(X509ContentType.Pfx), null);
}

/// <summary>
/// Writes a private key readable only by this user.
/// </summary>
/// <remarks>
/// File.WriteAllText takes the umask, which on most machines leaves a key world-readable. These are
/// throwaways, but a sample is read as an example of how to do it.
/// </remarks>
static void WritePrivateKey(string path, string pem)
{
    var options = new FileStreamOptions { Mode = FileMode.Create, Access = FileAccess.Write };

    if (!OperatingSystem.IsWindows())
    {
        options.UnixCreateMode = UnixFileMode.UserRead | UnixFileMode.UserWrite;
    }

    using var stream = new FileStream(path, options);
    using var writer = new StreamWriter(stream);

    writer.Write(pem);
}

/// <summary>
/// Marks an endpoint as requiring a client certificate.
/// </summary>
/// <remarks>
/// The ioxide engine reads <see cref="RequireCertificate"/> and lets OpenSSL (or ngtcp2 on HTTP/3)
/// validate the offered chain against the configured client CA, so a bad chain is refused before a
/// request exists and <see cref="Validate"/> is never called. Returning true here would not admit
/// anyone the CA had already rejected.
/// </remarks>
internal sealed class RequireClientCertificate : ICertificateValidator
{
    public bool RequireCertificate => true;

    public X509RevocationMode RevocationCheck => X509RevocationMode.NoCheck;

    public bool Validate(X509Certificate? certificate, X509Chain? chain, SslPolicyErrors policyErrors) => true;
}
