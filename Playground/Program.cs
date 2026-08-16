using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

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
//
//     dotnet run -c Release --project Playground
//
//     curl http://localhost:8080/ok
//     curl --http2-prior-knowledge http://localhost:8081/ok
//     curl --http1.1 http://localhost:8082/ok
//     curl -k --http1.1 https://localhost:8443/ok
//     curl -k --http2 https://localhost:8443/ok
//     curl -k --http3-only https://localhost:8443/ok
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
                  },

                  // Bytes of QPACK dynamic table offered to HTTP/3 clients. 0 keeps every header
                  // literal, which costs bytes but can never stall a stream on a table update. In
                  // practice only browsers advertise a table of their own.
                  QpackDynamicTableCapacity = 4096,
                  QpackBlockedStreams = 100,

                  // HTTP/3 is terminated by ngtcp2, which loads PEM from disk. Name the files here
                  // and nothing is written; leave them out and the bound certificate is exported to
                  // an owner-only temporary directory for the lifetime of the process.
                  //
                  // Http3CertificatePath = "/etc/ssl/site.crt",
                  // Http3KeyPath         = "/etc/ssl/site.key",

                  // Mutual TLS, enforced on all three protocols. Clients are asked for a
                  // certificate and validated against this bundle.
                  //
                  // ClientCaPath             = "/etc/ssl/clients.pem",
                  // RequireClientCertificate = true,
              })
          .Handler(app)
          .Bind(IPAddress.Loopback, 8080)
          .Bind(IPAddress.Loopback, 8081)
          .Bind(IPAddress.Loopback, 8082)
          .Bind(IPAddress.Loopback, 8443, certificate)
          .RunAsync();

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
