using System;
using System.IO;
using System.Net;

using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Api.Protocol
{
    
    public interface IClientHandler
    {

        /// <summary>
        /// The server this handler relates to.
        /// </summary>
        IServer Server { get; }

        /// <summary>
        /// The IP of the connected client.
        /// </summary>
        IPAddress IPAddress { get; }
        
    }

}
