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

// IPv6Any rather than Loopback: a browser resolves "localhost" itself and prefers ::1, so an
// IPv4-only listener never sees a packet from one. TCP masks this by falling back to IPv4, QUIC
// does not, and the result looks like a broken HTTP/3 server rather than a bind that is too narrow.
//
// HTTP/3 first, so it is listening before anything advertises it.
var h3 = Host.Create(qpackDynamicTableCapacity: QpackCapacity)
             .Handler(content)
             .Logging(NullLoggerFactory.Instance, logRequests: false)
             .Bind(IPAddress.IPv6Any, Port, certificate);

await h3.StartAsync();

// HTTP/1.1, advertising the endpoint above. The port has to match, and nothing checks that it does.
await GenHTTP.Engine.Internal.Host.Create()
             .Handler(content)
             .Add(AltSvc.To(Port))
             .Logging(NullLoggerFactory.Instance, logRequests: false)
             .Bind(IPAddress.IPv6Any, Port, certificate)
             .RunAsync();

static X509Certificate2 CreateDevelopmentCertificate()
{
    // A PKCS#12 issued by a local CA that the browser already trusts. This is the only arrangement
    // Chrome accepts: it dropped support for directly-trusted leaf certificates, so the ASP.NET
    // development certificate below (CA:FALSE, self-signed) can never satisfy it however it is
    // marked in the trust store. Point PLAYGROUND_CERT at a mkcert-style bundle to use one.
    //
    // It matters because a certificate the browser merely tolerates is not enough: an origin with
    // certificate errors has its Alt-Svc ignored outright, so HTTP/3 stays unreachable.
    if (Environment.GetEnvironmentVariable("PLAYGROUND_CERT") is { Length: > 0 } path && File.Exists(path))
    {
        return X509CertificateLoader.LoadPkcs12FromFile(path, null);
    }

    // Prefer the ASP.NET development certificate if one exists. It is the same across runs, so the
    // SPKI hash below stays stable, and `dotnet dev-certs https --trust` is enough for Firefox and
    // curl, which do honour a trusted leaf.
    if (FindAspNetDevelopmentCertificate() is { } trusted)
    {
        return trusted;
    }

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

/// <summary>
/// The ASP.NET development certificate, if one is installed and still valid. Identified by the
/// extension the tooling stamps on it rather than by its subject, which anything could claim.
/// </summary>
static X509Certificate2? FindAspNetDevelopmentCertificate()
{
    const string AspNetHttpsOid = "1.3.6.1.4.1.311.84.1.1";

    using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
    store.Open(OpenFlags.ReadOnly);

    // The NEWEST valid one, because that is the one `dotnet dev-certs https --trust` marks. Taking
    // whichever the store happened to enumerate first served an older certificate that was equally
    // valid and entirely untrusted, which a browser reports as an ordinary certificate error.
    return store.Certificates
                .Where(candidate => candidate.HasPrivateKey
                                    && candidate.NotAfter > DateTime.Now
                                    && candidate.Extensions.Any(e => e.Oid?.Value == AspNetHttpsOid))
                .OrderByDescending(candidate => candidate.NotAfter)
                .FirstOrDefault();
}

// What Chrome's --ignore-certificate-errors-spki-list wants: base64 of the SHA-256 of the
// certificate's public key info.
static string SpkiHash(X509Certificate2 certificate)
    => Convert.ToBase64String(SHA256.HashData(certificate.PublicKey.ExportSubjectPublicKeyInfo()));
