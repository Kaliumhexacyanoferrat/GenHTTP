using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Core.Infrastructure.Endpoints
{

    internal class SimpleCertificateProvider : ICertificateProvider
    {

        #region Get-/Setters

        internal X509Certificate Certificate { get; }

        #endregion

        #region Initialization

        internal SimpleCertificateProvider(X509Certificate certificate)
        {
            Certificate = certificate;
        }

        #endregion

        #region Functionaliy

        public X509Certificate Provide(string? host)
        {
            return Certificate;
        }

        #endregion

    }

}
