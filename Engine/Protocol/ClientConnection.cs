﻿using System.Net;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Protocol
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
