using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace GenHTTP.Api.Protocol
{
    
    public class Forwarding
    {

        #region Get-/Setters

        public IPAddress? For { get; }

        public string? Host { get; }

        public ClientProtocol? Protocol { get; }

        #endregion

        #region Initialization

        public Forwarding(IPAddress? forIP, string? host, ClientProtocol? protocol)
        {
            For = forIP;
            Host = host;
            Protocol = protocol;
        }

        #endregion

    }

}
