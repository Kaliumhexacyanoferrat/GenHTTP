using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Core.Infrastructure.Endpoints
{

    internal class SimpleCertificateProvider : ICertificateProvider
    {

        #region Get-/Setters

        internal X509Certificate2 Certificate { get; }

        internal string Host { get; }

        #endregion

        #region Initialization

        internal SimpleCertificateProvider(string host, X509Certificate2 certificate)
        {
            Certificate = certificate;
            Host = host;
        }

        #endregion

        #region Functionaliy

        public X509Certificate2? Provide(string? host) => Certificate;

        #endregion

    }

}
