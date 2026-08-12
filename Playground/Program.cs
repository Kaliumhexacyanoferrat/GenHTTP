using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using GenHTTP.Engine.InternalH3Experimental;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

using Microsoft.Extensions.Logging.Abstractions;

//   curl -k https://localhost:8443/hello              # HTTP/1.1, over TCP
//   curl -k --http3 https://localhost:8443/hello      # HTTP/3, over QUIC
//   curl -k -i https://localhost:8443/hello | grep -i alt-svc
//
// TCP:8443 and UDP:8443 are different sockets, so both engines bind the same number.

// One number, two sockets: the HTTP/1.1 host binds TCP 8443 and the HTTP/3 host binds
// UDP 8443. They do not collide, and Alt-Svc can then advertise the same port the client is
// already talking to.
const ushort Port = 8443;

var app = Layout.Create()
                .Add("hello", Content.From(Resource.FromString("Hello World!")));

var certificate = CreateDevelopmentCertificate();

// HTTP/3 first, so it is listening before anything advertises it.
//
// Request logging is off on both hosts: it writes a console line per request, which is enough to
// dominate a throughput measurement. Drop the Logging call to get it back.
var h3 = Host.Create()
             .Handler(app)
             .Logging(NullLoggerFactory.Instance, logRequests: false)
             .Bind(IPAddress.Loopback, Port, certificate);

await h3.StartAsync();

// HTTP/1.1, advertising the endpoint above. The port has to match, and nothing checks that it does:
// get it wrong and clients simply never upgrade.
await GenHTTP.Engine.Internal.Host.Create()
             .Handler(app)
             .Add(AltSvc.To(Port))
             .Logging(NullLoggerFactory.Instance, logRequests: false)
             .Bind(IPAddress.Loopback, Port, certificate)
             .RunAsync();

static X509Certificate2 CreateDevelopmentCertificate()
{
    using var key = RSA.Create(2048);

    var request = new CertificateRequest("CN=localhost", key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

    // Without a SAN nothing modern will verify this, only skip it with -k.
    var names = new SubjectAlternativeNameBuilder();
    names.AddDnsName("localhost");
    names.AddIpAddress(IPAddress.Loopback);
    request.CertificateExtensions.Add(names.Build());

    using var generated = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1));

    // The private key has to come back through a PFX round-trip before a TLS stack will use it.
    return X509CertificateLoader.LoadPkcs12(generated.Export(X509ContentType.Pfx), null);
}
