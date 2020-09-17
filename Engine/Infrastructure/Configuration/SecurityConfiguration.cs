using System.Security.Authentication;

using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Engine.Infrastructure.Configuration
{

    internal class SecurityConfiguration
    {

        #region Get-/Setters

        internal SslProtocols Protocols { get; }

        internal ICertificateProvider Certificate { get; }

        #endregion

        #region Initialization

        internal SecurityConfiguration(ICertificateProvider certificate, SslProtocols protocols)
        {
            Protocols = protocols;
            Certificate = certificate;
        }

        #endregion

    }

}
