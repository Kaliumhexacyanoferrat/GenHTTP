using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using GenHTTP.Engine.Ioxide;

using GenHTTP.Modules.Files;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

// The namespace and the class share a name, so the class needs an alias to be reachable.
using IoxideFilesModule = GenHTTP.Modules.IoxideFiles.IoxideFiles;

// One host, three ports, showing every arrangement the engine allows. Protocols are configured per
// port: HTTP/1.1 and HTTP/2 share a TCP socket, and HTTP/3 is a UDP socket on the same port number,
// so ports can be shared or separated however you like.
//
//   http://localhost:8080    HTTP/1.1 only
//   http://localhost:8081    HTTP/2 only, without TLS (h2c, for a client using prior knowledge)
//   https://localhost:8443   all three at once - HTTP/1.1 and HTTP/2 over TCP, HTTP/3 over UDP
//
// Every port is independent, so 8081 serves ONLY HTTP/2 - an HTTP/1.1 client is turned away there.
// Give a port Http1AndHttp2 and it serves both, one connection at a time: ALPN picks during the
// handshake on a secure port, and the HTTP/2 connection preface picks on a plaintext one. That is
// what 8443 does below.
//
//     dotnet run -c Release --project Playground
//
//     curl http://localhost:8080/ok
//     curl --http2-prior-knowledge http://localhost:8081/ok
//     curl -k --http1.1 https://localhost:8443/ok
//     curl -k --http2 https://localhost:8443/ok
//     curl -k --http3-only https://localhost:8443/ok
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

// One io_uring reactor per core by default. GENHTTP_REACTORS lowers it, which is what a benchmark
// wants: a server sized to every core leaves none for the load generator, and the run then measures
// the generator rather than the server.
var reactors = int.TryParse(Environment.GetEnvironmentVariable("GENHTTP_REACTORS"), out var r) ? r : Environment.ProcessorCount;

await Host.Create(
              configure: c => c with { ReactorCount = reactors },
              options: new IoxideOptions
              {
                  // What a port serves unless named below.
                  Protocols = IoxideProtocols.Http1,

                  ProtocolsByPort =
                  {
                      // h2c only: an HTTP/1.1 client is turned away here. Http1AndHttp2 would
                      // serve both on this one port, decided by the connection preface.
                      [8081] = IoxideProtocols.Http2,
                      [8443] = IoxideProtocols.All,     // h1 + h2 over TCP, h3 over UDP, one number
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
