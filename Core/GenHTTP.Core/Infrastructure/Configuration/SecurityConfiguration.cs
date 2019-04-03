using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace GenHTTP.Core.Infrastructure.Configuration
{

    internal class SecurityConfiguration
    {

        #region Get-/Setters

        internal SslProtocols Protocols { get; }

        internal X509Certificate Certificate { get; }
        
        #endregion

        #region Initialization

        internal SecurityConfiguration(X509Certificate certificate, SslProtocols protocols)
        {
            Protocols = protocols;
            Certificate = certificate;
        }

        #endregion

    }

}
