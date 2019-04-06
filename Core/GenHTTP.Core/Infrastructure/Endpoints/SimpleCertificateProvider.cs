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

        internal X509Certificate2 Certificate { get; }

        #endregion

        #region Initialization

        internal SimpleCertificateProvider(X509Certificate2 certificate)
        {
            Certificate = certificate;
        }

        #endregion

        #region Functionaliy

        public X509Certificate2 Provide(string? host)
        {
            return Certificate;
        }

        #endregion

    }

}
