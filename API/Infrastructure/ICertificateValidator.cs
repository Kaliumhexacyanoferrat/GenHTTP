using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace GenHTTP.Api.Infrastructure;

public interface ICertificateValidator
{

    bool ForceClientCertificate { get; }

    X509RevocationMode RevocationMode { get; }

    bool Validate(X509Certificate? certificate, X509Chain? chain, SslPolicyErrors policyErrors);

}
