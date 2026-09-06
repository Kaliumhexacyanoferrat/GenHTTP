using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace GenHTTP.Playground;

/// <summary>
/// Throwaway PEM certificates in ./certs, so the sample runs with no setup. A deployment points the
/// providers at the files its ACME client already writes and deletes all of this.
/// </summary>
internal static class Certs
{
    private static readonly string Dir = Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "certs")).FullName;

    /// <summary>
    /// Mints a self-signed certificate for one name, overwriting any earlier one. Called again on
    /// SIGHUP, which is what gives the rotation something new to install.
    /// </summary>
    public static (string Certificate, string Key) Server(string dnsName)
    {
        using var key = RSA.Create(2048);

        var request = new CertificateRequest($"CN={dnsName}", key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        // Without a subject alternative name nothing modern will verify this, only skip the check.
        var names = new SubjectAlternativeNameBuilder();
        names.AddDnsName(dnsName);
        names.AddIpAddress(IPAddress.Loopback);
        request.CertificateExtensions.Add(names.Build());

        using var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1));

        return Write(dnsName, certificate, key.ExportPkcs8PrivateKeyPem());
    }

    /// <summary>
    /// A CA, a client it signs and an impostor it does not, so the mutual TLS port can be tried
    /// both ways. Returns the CA's path.
    /// </summary>
    public static string ClientPki()
    {
        // One instant for all of them: read the clock per certificate and a leaf can outlive its
        // issuer by a second, which is refused.
        var from = DateTimeOffset.UtcNow.AddDays(-1);
        var until = from.AddYears(1);

        using var caKey = RSA.Create(2048);

        var caRequest = new CertificateRequest("CN=playground client CA", caKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        caRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));

        using var ca = caRequest.CreateSelfSigned(from, until);

        Issue("client", "CN=alice", ca, from, until);
        Issue("impostor", "CN=mallory", null, from, until);

        return Write("client-ca", ca, keyPem: null).Certificate;
    }

    private static void Issue(string name, string subject, X509Certificate2? issuer, DateTimeOffset from, DateTimeOffset until)
    {
        using var key = RSA.Create(2048);

        var request = new CertificateRequest(subject, key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        using var certificate = issuer is null
            ? request.CreateSelfSigned(from, until)
            : request.Create(issuer, from, until, Guid.NewGuid().ToByteArray());

        Write(name, certificate, key.ExportPkcs8PrivateKeyPem());
    }

    private static (string Certificate, string Key) Write(string name, X509Certificate2 certificate, string? keyPem)
    {
        var certPath = Path.Combine(Dir, $"{name}.crt");
        var keyPath = Path.Combine(Dir, $"{name}.key");

        File.WriteAllText(certPath, certificate.ExportCertificatePem());

        if (keyPem is not null)
        {
            // WriteAllText takes the umask, which usually leaves a key world-readable. Throwaways,
            // but a sample is read as an example of how to do it.
            var options = new FileStreamOptions { Mode = FileMode.Create, Access = FileAccess.Write };

            if (!OperatingSystem.IsWindows())
            {
                options.UnixCreateMode = UnixFileMode.UserRead | UnixFileMode.UserWrite;
            }

            using var stream = new FileStream(keyPath, options);
            stream.Write(Encoding.ASCII.GetBytes(keyPem));
        }

        return (certPath, keyPath);
    }
}
