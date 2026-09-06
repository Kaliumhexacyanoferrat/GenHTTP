using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace GenHTTP.Testing.Acceptance.Engine.Ioxide;

/// <summary>
/// Throwaway PEM on disk, which is the only form HTTP/3 takes and the form certificate rotation
/// reads again. Each call overwrites the pair, so a test can rotate by minting the same name twice.
/// </summary>
public static class IoxideCertificates
{
    private static readonly string Dir =
        Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "genhttp-ioxide-tests")).FullName;

    /// <summary>Mints a self-signed certificate for one name and returns the two paths.</summary>
    public static (string Certificate, string Key) Create(string dnsName, string? into = null)
    {
        using var key = RSA.Create(2048);

        var request = new CertificateRequest($"CN={dnsName}", key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        var names = new SubjectAlternativeNameBuilder();
        names.AddDnsName(dnsName);
        names.AddIpAddress(IPAddress.Loopback);
        request.CertificateExtensions.Add(names.Build());

        using var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1));

        var stem = Path.Combine(into ?? Dir, dnsName);

        File.WriteAllText($"{stem}.crt", certificate.ExportCertificatePem());
        File.WriteAllText($"{stem}.key", key.ExportPkcs8PrivateKeyPem());

        return ($"{stem}.crt", $"{stem}.key");
    }

    /// <summary>A directory of its own, so one test's rotation cannot disturb another's files.</summary>
    public static string Isolated()
        => Directory.CreateDirectory(Path.Combine(Dir, Guid.NewGuid().ToString("n"))).FullName;

    /// <summary>The serial a peer is currently being served on this port, over TLS.</summary>
    public static async Task<string> SerialAsync(ushort port, string host = "localhost")
    {
        using var client = new System.Net.Sockets.TcpClient();

        await client.ConnectAsync(IPAddress.Loopback, port);

        await using var stream = new System.Net.Security.SslStream(client.GetStream(), false, (_, _, _, _) => true);

        await stream.AuthenticateAsClientAsync(new System.Net.Security.SslClientAuthenticationOptions
        {
            TargetHost = host,
        });

        return stream.RemoteCertificate is { } certificate
            ? new X509Certificate2(certificate).SerialNumber
            : throw new InvalidOperationException($"Port {port} served no certificate for {host}.");
    }

    /// <summary>The subject a peer is served for one name, which is what SNI decides.</summary>
    public static async Task<string> SubjectAsync(ushort port, string host)
    {
        using var client = new System.Net.Sockets.TcpClient();

        await client.ConnectAsync(IPAddress.Loopback, port);

        await using var stream = new System.Net.Security.SslStream(client.GetStream(), false, (_, _, _, _) => true);

        await stream.AuthenticateAsClientAsync(new System.Net.Security.SslClientAuthenticationOptions
        {
            TargetHost = host,
        });

        return stream.RemoteCertificate?.Subject ?? throw new InvalidOperationException($"Port {port} served no certificate for {host}.");
    }

}
