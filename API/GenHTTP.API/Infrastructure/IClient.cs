using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace GenHTTP.Api.Infrastructure
{
    
    /// <summary>
    /// The remote client which requests a resource from the server.
    /// </summary>
    public interface IClient
    {

        /// <summary>
        /// The IP address of the remotely connected client.
        /// </summary>
        IPAddress IPAddress { get; }

        /// <summary>
        /// The port of the remotely connected client.
        /// </summary>
        ushort Port { get; }

    }

}
