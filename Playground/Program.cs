using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using GenHTTP.Engine.InternalH3Experimental;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

//   curl -k https://localhost:8443/hello              # HTTP/1.1, over TCP
//   curl -k --http3 https://localhost:8443/hello      # HTTP/3, over QUIC
//   curl -k -i https://localhost:8443/hello | grep -i alt-svc
//
// TCP:8443 and UDP:8443 are different sockets, so both engines bind the same number.

const ushort H3Port = 8443;

var app = Layout.Create()
                .Add("hello", Content.From(Resource.FromString("Hello World!")));

var certificate = CreateDevelopmentCertificate();

// HTTP/3 first, so it is listening before anything advertises it.
var h3 = Host.Create()
             .Handler(app)
             .Bind(IPAddress.Loopback, H3Port, certificate);

await h3.StartAsync();

// HTTP/1.1, advertising the endpoint above. The port has to match, and nothing checks that it does:
// get it wrong and clients simply never upgrade.
await GenHTTP.Engine.Internal.Host.Create()
             .Handler(app)
             .Add(AltSvc.To(H3Port))
             .Bind(IPAddress.Loopback, H3Port, certificate)
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
