using System.Net;

namespace GenHTTP.Core.Infrastructure.Configuration
{

    internal class EndPointConfiguration
    {

        #region Get-/Setters

        internal IPAddress Address { get; }

        internal ushort Port { get; }

        internal SecurityConfiguration? Security { get; }

        #endregion

        #region Initialization

        internal EndPointConfiguration(IPAddress address, ushort port, SecurityConfiguration? security)
        {
            Address = address;
            Port = port;
            Security = security;
        }

        #endregion

    }

}
