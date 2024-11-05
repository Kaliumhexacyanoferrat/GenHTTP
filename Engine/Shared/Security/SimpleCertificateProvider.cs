using System.Security.Cryptography.X509Certificates;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Engine.Shared.Security;

internal sealed class SimpleCertificateProvider : ICertificateProvider
{

    #region Initialization

    internal SimpleCertificateProvider(X509Certificate2 certificate)
    {
        Certificate = certificate;
    }

    #endregion

    #region Get-/Setters

    internal X509Certificate2 Certificate { get; }

    #endregion

    #region Functionaliy

    public X509Certificate2 Provide(string? host) => Certificate;

    #endregion

}
