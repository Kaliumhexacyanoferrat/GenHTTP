using System.Security.Cryptography.X509Certificates;

using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Engine.Shared.Security;

public sealed class SimpleCertificateProvider : ICertificateProvider
{

    #region Initialization

    public SimpleCertificateProvider(X509Certificate2 certificate)
    {
        Certificate = certificate;
    }

    #endregion

    #region Get-/Setters

    public X509Certificate2 Certificate { get; }

    #endregion

    #region Functionaliy

    public X509Certificate2 Provide(string? host) => Certificate;

    #endregion

}
