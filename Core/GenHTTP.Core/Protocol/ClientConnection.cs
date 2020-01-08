using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using System.Net;

namespace GenHTTP.Core.Protocol
{

    internal class ClientConnection : IClientConnection
    {

        #region Get-/Setters

        public IPAddress IPAddress { get; }
        
        public ClientProtocol? Protocol { get; }

        public string? Host { get; }


        #endregion

        #region Initialization

        internal ClientConnection(IPAddress address, ClientProtocol? protocol, string? host)
        {
            IPAddress = address;
            Protocol = protocol;
            Host = host;
        }

        #endregion

    }

}
