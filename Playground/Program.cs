using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using GenHTTP.Engine.InternalH3Experimental;

using GenHTTP.Modules.StaticWebsites;
using GenHTTP.Modules.IO;

using Microsoft.Extensions.Logging.Abstractions;

// Serves wwwroot over HTTP/1.1 and HTTP/3 at once, so a browser can be watched deciding which to
// use. The page reports the protocol it arrived over.
//
//   dotnet run -c Release --project Playground
//   curl -k https://localhost:8443/                     # HTTP/1.1, and an Alt-Svc header
//   snap run curl -k --http3-only https://localhost:8443/
//
// A browser will not speak HTTP/3 to an untrusted certificate. Chrome can be told to anyway, using
// the SPKI hash this prints on startup:
//
//   google-chrome --origin-to-force-quic-on=localhost:8443 \
//                 --ignore-certificate-errors-spki-list=<printed below> \
//                 https://localhost:8443/
//
// One number, two sockets: the HTTP/1.1 host binds TCP 8443 and the HTTP/3 host binds UDP 8443.
const ushort Port = 8443;

// Bytes of QPACK dynamic table advertised to clients. Nonzero here on purpose: this playground
// exists partly to find out whether a browser uses one, since curl and .NET's client do not.
const int QpackCapacity = 4096;

// Beside the binary, since wwwroot is copied to the output rather than served from the source
// tree - a relative path would resolve against whatever directory you happened to run from.
string root = Path.Combine(AppContext.BaseDirectory, "wwwroot");

var content = StaticWebsite.From(ResourceTree.FromDirectory(root));

var certificate = CreateDevelopmentCertificate();

Console.WriteLine($"SPKI hash: {SpkiHash(certificate)}");

// HTTP/3 first, so it is listening before anything advertises it.
var h3 = Host.Create(QpackCapacity)
             .Handler(content)
             .Logging(NullLoggerFactory.Instance, logRequests: false)
             .Bind(IPAddress.Loopback, Port, certificate);

await h3.StartAsync();

// HTTP/1.1, advertising the endpoint above. The port has to match, and nothing checks that it does.
await GenHTTP.Engine.Internal.Host.Create()
             .Handler(content)
             .Add(AltSvc.To(Port))
             .Logging(NullLoggerFactory.Instance, logRequests: false)
             .Bind(IPAddress.Loopback, Port, certificate)
             .RunAsync();

static X509Certificate2 CreateDevelopmentCertificate()
{
    using var key = RSA.Create(2048);

    var request = new CertificateRequest("CN=localhost", key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

    // Without a SAN nothing modern will verify this, only skip it.
    var names = new SubjectAlternativeNameBuilder();
    names.AddDnsName("localhost");
    names.AddIpAddress(IPAddress.Loopback);
    request.CertificateExtensions.Add(names.Build());

    using var generated = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1));

    // The private key has to come back through a PFX round-trip before a TLS stack will use it.
    return X509CertificateLoader.LoadPkcs12(generated.Export(X509ContentType.Pfx), null);
}

// What Chrome's --ignore-certificate-errors-spki-list wants: base64 of the SHA-256 of the
// certificate's public key info.
static string SpkiHash(X509Certificate2 certificate)
    => Convert.ToBase64String(SHA256.HashData(certificate.PublicKey.ExportSubjectPublicKeyInfo()));
