using System.Security.Cryptography.X509Certificates;
using GenHTTP.Modules.IO;

namespace GenHTTP.Testing.Acceptance.Utilities;

public static class Security
{

    public static async ValueTask<X509Certificate2> GetCertificateAsync()
    {
        await using var stream = await Resource.FromAssembly("Certificate.pfx").Build().GetContentAsync();

        using var mem = new MemoryStream();

        await stream.CopyToAsync(mem);
#if NET8_0
        return new X509Certificate2(mem.ToArray());
#else
        return X509CertificateLoader.LoadPkcs12(mem.ToArray(), null);
#endif
    }

}
